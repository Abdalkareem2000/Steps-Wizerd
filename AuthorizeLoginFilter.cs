namespace DCCO.Presentation.DCCO.Web.UsersSecurity
{
    public class AuthorizeLoginFilter : IActionFilter
    {
        public static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }
        public void OnActionExecuted(ActionExecutedContext context) { }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = (BaseController)context.Controller;
            if (_httpContextAccessor.HttpContext.Session.GetString(SessionKey.IDNumber) != null && _httpContextAccessor.HttpContext.Session.GetString(SessionKey.SessionId) != null)
            {
                if (!controller.ToString().Contains(Security.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(SessionKey.Role))))
                    context.Result = controller.RedirectToAction("AccessDenied", "Shared");
                IUserService userService = (IUserService)_httpContextAccessor.HttpContext.RequestServices.GetService(typeof(IUserService));

                var ssesionId = Security.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(SessionKey.SessionId));
                var iDNumber = Security.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(SessionKey.IDNumber));
                // check to see if your ID in the Logins table has LoggedIn = true - if so, continue, otherwise, redirect to Login page.
                if (userService.IsUserStillLogin(iDNumber, ssesionId))
                {
                    // check to see if your user ID is being used elsewhere under a different session ID
                    if (!userService.IsUserLoggedOnElseWhere(iDNumber, ssesionId))
                    {
                        return;
                    }
                    else
                    {
                        // if it is being used elsewhere, update all their Logins records to LoggedIn = false, except for your session ID
                        userService.LogEveryoneElseOut(iDNumber, ssesionId);
                        return;
                    }
                }
                else
                {
                    context.Result = controller.RedirectToAction("Login", "Home");
                }
            }
            else
            {
                context.Result = controller.RedirectToAction("Login", "Home");
            }
        }
    }
}