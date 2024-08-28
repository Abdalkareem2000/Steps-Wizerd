using System;
using System.IO;
using System.Threading.Tasks;

namespace DCCO.Presentation.DCCO.Web.Controllers
{
    [ServiceFilter(typeof(AuthorizeLoginFilter))]
    public class MakerController : BaseController
    {
        #region Fileds & Ctor
        private readonly IApplicationLogger _logger;
        private readonly IMakerService _makerService;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ISharedService _sharedService;
        private readonly ISessionService _sessionService;

        public MakerController(IApplicationLogger logger, IMakerService makerService, ICompositeViewEngine viewEngine, ISharedService sharedService, ISessionService sessionService)
        {
            _logger = logger;
            _makerService = makerService;
            _viewEngine = viewEngine;
            _sharedService = sharedService;
            _sessionService = sessionService;
        }
        #endregion

        #region Methods
        public async Task<IActionResult> ApplicationJourney()
        {
            //Prepare VM
            AccountInfoUpdateDTO accountInfoUpdateDTO = new AccountInfoUpdateDTO();

            accountInfoUpdateDTO.ActiveStep = _sessionService.GetInt(SessionKey.ActiveStep) ?? (int)ApplicationSteps.EntityBasicInformation;
            _sessionService.SetInt(SessionKey.ActiveStep, accountInfoUpdateDTO.ActiveStep);

            accountInfoUpdateDTO = await _makerService.PrepareVM(accountInfoUpdateDTO);

            ViewData["Partial"] = await ConvertViewToString(this.ControllerContext, PartialView($"ApplicationSteps/_{Enum.GetName(typeof(ApplicationSteps), accountInfoUpdateDTO.ActiveStep)}", accountInfoUpdateDTO), _viewEngine);
            return View(accountInfoUpdateDTO);
        }
        [HttpPost]
        public async Task<IActionResult> ApplicationJourney(AccountInfoUpdateDTO accountInfoUpdateDTO)
        {
            await _logger.LogInformation($"Step {Enum.GetName(typeof(ApplicationSteps), accountInfoUpdateDTO.ActiveStep)} ApplicationJourney", $"Getting Response from front Action {Enum.GetName(typeof(JourneyActions), accountInfoUpdateDTO.JourneyAction)}", ActionTypes.ApplicationJourney.ToString(), accountInfoUpdateDTO, null, HttpStatusCode.Ok, LogServerity.Info, _sessionService.GetString(SessionKey.IDNumber), false);
            if (accountInfoUpdateDTO.JourneyAction == (int)JourneyActions.Continue || accountInfoUpdateDTO.JourneyAction == (int)JourneyActions.AddNewParty ||
                accountInfoUpdateDTO.JourneyAction == (int)JourneyActions.Add_UpdatePassiveNFFEEntity || accountInfoUpdateDTO.JourneyAction == (int)JourneyActions.DeletePassiveNFFEEntity ||
                accountInfoUpdateDTO.JourneyAction == (int)JourneyActions.Add_UpdatePassiveNFFEController || accountInfoUpdateDTO.JourneyAction == (int)JourneyActions.DeletePassiveNFFEController)
            {
                //Validate application
                var validModel = await _sharedService.ValidateObj(accountInfoUpdateDTO);
                if (!validModel.Succeeded)
                {
                    await _logger.LogInformation($"Step {Enum.GetName(typeof(ApplicationSteps), accountInfoUpdateDTO.ActiveStep)} ApplicationJourney", $"validModel is Not valid", ActionTypes.ApplicationJourney.ToString(), validModel.BrokenRules, null, HttpStatusCode.BusinessRuleViolation, LogServerity.Info, _sessionService.GetString(SessionKey.IDNumber), false);
                    return Json(new { isSuccess = false, resultCode = (int)HttpStatusCode.BusinessRuleViolation, brokenRoles = validModel.BrokenRules });
                }

                //Save app
                await _makerService.SaveSteps(accountInfoUpdateDTO);

                //PrepareVM
                if (accountInfoUpdateDTO.ActiveStep == (int)ApplicationSteps.AddParty)
                    accountInfoUpdateDTO.ActiveStep -= 1;
                else if (accountInfoUpdateDTO.ActiveStep == (int)ApplicationSteps.EntityPartiesDetails && accountInfoUpdateDTO.JourneyAction != (int)JourneyActions.AddNewParty)
                    accountInfoUpdateDTO.ActiveStep += 2;
                else if (accountInfoUpdateDTO.JourneyAction == (int)JourneyActions.Add_UpdatePassiveNFFEEntity || accountInfoUpdateDTO.JourneyAction == (int)JourneyActions.DeletePassiveNFFEEntity ||
                    accountInfoUpdateDTO.JourneyAction == (int)JourneyActions.Add_UpdatePassiveNFFEController || accountInfoUpdateDTO.JourneyAction == (int)JourneyActions.DeletePassiveNFFEController)
                { }
                else
                    accountInfoUpdateDTO.ActiveStep += 1;

                accountInfoUpdateDTO = await _makerService.PrepareVM(accountInfoUpdateDTO);
            }
            else if (accountInfoUpdateDTO.JourneyAction == (int)JourneyActions.Back)
            {
                //PrepareVM
                if (accountInfoUpdateDTO.ActiveStep == (int)ApplicationSteps.EntityAlertsAndAddress)
                    accountInfoUpdateDTO.ActiveStep -= 2;
                else
                    accountInfoUpdateDTO.ActiveStep -= 1;
                accountInfoUpdateDTO = await _makerService.PrepareVM(accountInfoUpdateDTO);
            }
            else if (accountInfoUpdateDTO.JourneyAction == (int)JourneyActions.SaveForLater)
            {
                await _makerService.SaveSteps(accountInfoUpdateDTO);
            }
            else if (accountInfoUpdateDTO.JourneyAction == (int)JourneyActions.Submit)
            {
                await _makerService.SaveSteps(accountInfoUpdateDTO);
            }
            else
            {
                return Json(new { isSuccess = false, resultCode = (int)HttpStatusCode.BadRequest });
            }

            _sessionService.SetInt(SessionKey.ActiveStep, accountInfoUpdateDTO.ActiveStep);
            //Build partialView for next or previous step
            return Json(new
            {
                isSuccess = true,
                resultCode = 200,
                step = await ConvertViewToString(this.ControllerContext, PartialView($"ApplicationSteps/_{Enum.GetName(typeof(ApplicationSteps), accountInfoUpdateDTO.ActiveStep)}", accountInfoUpdateDTO), _viewEngine),
                next = accountInfoUpdateDTO.ActiveStep
            });
        }
        [HttpGet]
        public async Task<ActionResult> CallYaqeen(string iDNumber, string birthDateHijri)
        {
            try
            {
                var result = await _makerService.CallYaqeen(iDNumber, birthDateHijri);
                return Json(new { success = true, result = result });
            }
            catch (Exception ex)
            {
                await _logger.LogInformation($"Call Yaqeen", $"Throw Exception when CallYaqeen", ActionTypes.Yaqeen.ToString(), $"Exception Message :{ex.Message}\n Stack Trace :{ex.StackTrace}", null, HttpStatusCode.InternalError, LogServerity.Info, _sessionService.GetString(SessionKey.IDNumber), false);
                return Json(new { success = false });
            }
        }
        [HttpGet]
        public async Task<ActionResult> CallSaudiPost(string cRnumber)
        {
            try
            {
                var result = await _makerService.CallSaudiPost(cRnumber);
                return Json(new { success = true, result = result });
            }
            catch (Exception ex)
            {
                await _logger.LogInformation($"Call Yaqeen", $"Throw Exception when CallSaudiPost", ActionTypes.SaudiPost.ToString(), $"Exception Message :{ex.Message}\n Stack Trace :{ex.StackTrace}", null, HttpStatusCode.InternalError, LogServerity.Info, _sessionService.GetString(SessionKey.IDNumber), false);
                return Json(new { success = false });
            }
        }
        private async Task<string> ConvertViewToString(ControllerContext controllerContext, PartialViewResult pvr, ICompositeViewEngine _viewEngine)
        {
            using (StringWriter writer = new StringWriter())
            {
                ViewEngineResult vResult = _viewEngine.FindView(controllerContext, pvr.ViewName, false);
                ViewContext viewContext = new ViewContext(controllerContext, vResult.View, pvr.ViewData, pvr.TempData, writer, new HtmlHelperOptions());
                await vResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }
        #endregion
    }
}