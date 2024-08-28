
let activeStep = 0;

var Steps = {
    OnLoad: function() {
    var activeStep = $('#hdn-active-step').val();

    if (parseInt(activeStep) == 3)
            $('#spn-nxt-stp').html(stepsTitle[parseInt(activeStep)]);
        else if (parseInt(activeStep) == 2)
            $('#spn-nxt-stp').html(stepsTitle[parseInt(activeStep) + 2]);
        else
            $('#spn-nxt-stp').html(stepsTitle[parseInt(activeStep) + 1]);

    Steps.ButtonsSettings(activeStep);

    Steps.CheckedRadio();
    Steps.PrepareHijriCalendars();
},
    Action: function(status) {
        //if (!$('#formApplicationJourney').valid())
        //    return;
        $('#hdn-journey-action').val(status);

        // Post for Steps
        $.ajax({
    cache: false,
            type: 'POST',
            url: $('#formApplicationJourney').attr('action'),
            data: $('#formApplicationJourney').serialize(),
            dataType: 'json',
            success: function(data) {
            if (data.resultCode == 422)
            {
                    $('span[data-valmsg-for]').html('');
                for (var i = 0; i < data.brokenRoles.length; i++)
                {
                    var docReference = data.brokenRoles[i]["propertyName"];
                    var message = data.brokenRoles[i]["message"];
                    var errorElement = $("span[data-valmsg-for='" + docReference + "']");

                    errorElement.html(message);
                        $('.file-box-' + docReference).removeClass('uploaded').addClass('not-uploaded');

                    // Check if the error element is visible at the top of the page
                    if (errorElement.is (":visible") && errorElement.offset().top < $(window).scrollTop()) {
                            // Scroll the page to the top to make the error message visible
                            $("html, body").animate({ scrollTop: errorElement.offset().top }, 500);
                }
            }
        }
                else if (data.resultCode == 200)
        {
            if (status == journeyActions.Continue || status == journeyActions.Back || status == journeyActions.AddNewParty)
            {
                Steps.GoTo(data, true);
            }
            else if (status == journeyActions.Add_UpdatePassiveNFFEEntity || status == journeyActions.Add_UpdatePassiveNFFEController ||
                status == journeyActions.DeletePassiveNFFEEntity || status == journeyActions.DeletePassiveNFFEController)
            {
                location.reload();
            }
            else if (status == journeyActions.SaveForLater && data.url != '')
            {
                GeneralClass.AlertMessage(publicResources.ApplicationSavedSuccessFully, '', AlertsType.Info);
            }
        }
        else
        {
            GeneralClass.AlertMessage(publicResources.ErrorOccurred, '', AlertsType.Warning);
        }
    },
            error: function(e) {
        console.log(e.responseText);
    }
});
    },
    CheckedRadio: function() {
        $('input[type="radio"]:checked').each(function() {
            $(this).parent().addClass('checked');
    });

        $('input[type="radio"]').on('click', function() {
        var elementName = $(this).attr('name');
            $('input[type="radio"]').each(function() {
            if ($(this).attr('name') === elementName) {
                    $(this).parent().removeClass('checked');
            }
        });
            $(this).parent().addClass('checked');
    });
},
    ButtonsSettings: function(activeStep) {
    //Hide back
    if (activeStep == stepsEnum.EntityBasicInformation)
            $('#btn-back').addClass('d-none');
        else
            $('#btn-back').removeClass('d-none');

    if (activeStep != stepsEnum.Submit)
    {
            $('#btn-submit').addClass('d-none');
            $('#btn-next').removeClass('d-none');
    }
    else
    {
            $('#btn-submit').removeClass('d-none');
            $('#btn-next').addClass('d-none');
    }
},
    GoTo: function(data, isWithNxtStep) {
    var activeStep = data.next;
        $('#hdn-active-step').val(activeStep);
    if (isWithNxtStep)
    {
        if (parseInt(activeStep) == 3)
                $('#spn-nxt-stp').html(stepsTitle[parseInt(activeStep)]);
            else if (parseInt(activeStep) == 2)
                $('#spn-nxt-stp').html(stepsTitle[parseInt(activeStep) + 2]);
            else
                $('#spn-nxt-stp').html(stepsTitle[parseInt(activeStep) + 1]);
    }
        $('.main-steps').find('.progress-step').removeClass('is-active');
        $('.main-steps .progress-tracker li[data-step="' + data.stage + '"]').addClass('is-active');
        $('#dvSteps').html(data.step);
        $('#hdn-isMoveOAO').val(data.isOAO);
        $('#hdn-go-home').val(data.goToHome)
        Steps.ButtonsSettings(activeStep);
    Steps.CheckedRadio();
    Steps.PrepareHijriCalendars();
    window.scrollTo(0, 0);
},
    PrepareHijriCalendars: function() {
        $('.dateDOBHijri').each(function() {
        var local = _SystemLanguge;

        // Set locale based on language
        var local = "en-US";
        if (_SystemLanguge == 'ar')
        {
            local = "ar-SA";
        }

        var maxDate = '';
        var minDate = '';
        var showinDate = '';
        var today = new Date();
        var dd = today.getDate();
        var mm = today.getMonth() + 1;
        var yyyy = today.getFullYear();

        if (typeof $(this).attr('dataDate') != 'undefined') {
            var date = mm + '/' + dd + '/' + (yyyy - $(this).attr('dataDate').slice(1, -1));
            var newDate = date.split('/');
            if (newDate[0].length == 1)
                newDate[0] = "0" + newDate[0];
            if (newDate[1].length == 1)
                newDate[1] = "0" + newDate[1];

            maxDate = newDate[2] + '/' + newDate[0] + '/' + newDate[1];
            showinDate = (parseInt(newDate[2]) - 2) + '/' + newDate[0] + '/' + newDate[1];
        }

            $(this).hijriDatePicker({
        locale: local, // Set locale for Gregorian date picker
                hijri: true,
                format: "iDD/iMM/iYYYY",
                showSwitcher: false
            });

            $(this).next('.hijriDatePicker-toggle').hide();

            $(this).on('keypress', function(e) {
            var leng = $(this).val().length;

            if (window.event) {
                code = e.keyCode;
            } else
            {
                code = e.which;
            }

            var allowedCharacters = { 49: 1, 50: 2, 51: 3, 52: 4, 53: 5, 54: 6, 55: 7, 56: 8, 57: 9, 48: 0, 47: '/' }; /* KeyCodes for 1,2,3,4,5,6,7,8,9,/ */

        if (typeof allowedCharacters[code] === 'undefined' || /* Can only input 1,2,3,4,5,6,7,8,9 or / */
            (code == 47 && (leng < 2 || leng > 5 || leng == 3 || leng == 4)) ||
            ((leng == 2 || leng == 5) && code !== 47) || /* only can hit a / for 3rd pos. */
            leng == 10) /* only want 10 characters "12/45/7890" */
        {

                    event.preventDefault();

            return;
        }
    });
});

        $('.dateDOBGregorian').each(function() {
    var local = _SystemLanguge;

    // Set locale based on language
    var local = "en-US";
    if (_SystemLanguge == 'ar')
    {
        local = "ar-SA";
    }

    var maxDate = '';
    var minDate = '';
    var showinDate = '';
    var today = new Date();
    var dd = today.getDate();
    var mm = today.getMonth() + 1;
    var yyyy = today.getFullYear();

    if (typeof $(this).attr('dataDate') != 'undefined') {
        var date = mm + '/' + dd + '/' + (yyyy - $(this).attr('dataDate').slice(1, -1));
        var newDate = date.split('/');
        if (newDate[0].length == 1)
            newDate[0] = "0" + newDate[0];
        if (newDate[1].length == 1)
            newDate[1] = "0" + newDate[1];

        maxDate = newDate[2] + '/' + newDate[0] + '/' + newDate[1];
        showinDate = (parseInt(newDate[2]) - 2) + '/' + newDate[0] + '/' + newDate[1];
    }

            $(this).hijriDatePicker({
    locale: local, // Set locale for Gregorian date picker
                format: "YYYY-MM-DD",
                hijri: false,
                showSwitcher: false
            });
            $(this).on('keypress', function(e) {
        var leng = $(this).val().length;

        if (window.event) {
            code = e.keyCode;
        } else
        {
            code = e.which;
        }

        var allowedCharacters = { 49: 1, 50: 2, 51: 3, 52: 4, 53: 5, 54: 6, 55: 7, 56: 8, 57: 9, 48: 0, 47: '/' }; /* KeyCodes for 1,2,3,4,5,6,7,8,9,/ */

    if (typeof allowedCharacters[code] === 'undefined' || /* Can only input 1,2,3,4,5,6,7,8,9 or / */
        (code == 47 && (leng < 2 || leng > 5 || leng == 3 || leng == 4)) ||
        ((leng == 2 || leng == 5) && code !== 47) || /* only can hit a / for 3rd pos. */
        leng == 10) /* only want 10 characters "12/45/7890" */
    {

                    event.preventDefault();

        return;
    }
});
        });
    },
    ConvertHijriToGregroian: function(date) {
    var date = calendarIslamic.parseDate(calendarFormat, date);
    date = calendarGregorian.fromJD(date.toJD());
    return calendarGregorian.formatDate(calendarFormat, date);
},
    ConvertGregoianToHijri: function(date) {
    var date = calendarGregorian.parseDate(calendarFormat, date);
    date = calendarIslamic.fromJD(date.toJD());
    return calendarIslamic.formatDate(calendarFormat, date);
}
}