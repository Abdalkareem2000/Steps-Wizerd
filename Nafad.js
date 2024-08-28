var Nafad = {
    OnStartup: function () {

        //Set Timer
        var culture = window.location.href.includes('ar') ? 'ar' : 'en';
        $("#timer").countdown({ until: elapsedTime, format: 'S', onExpiry: Nafad.CheckNafadStatus(), culture: culture });

        if (elapsedTime != null && elapsedTime < 0)
            Nafad.CheckNafadStatus();

        //SignalR
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`/ nafadSignalR ? TransactionID =${transactionID}`)
            .build();

        connection.on("ReceiveNafadStatus", function () {
            Nafad.CheckNafadStatus();
        });

        connection.start()
            .then(function () {
                alert("Connected successfully");
            })
            .catch(function (err) {
                alert(err.toString());
            });
    },
    CheckNafadStatus: function () {
        $('#wait_overlay').show();

        $.ajax({
            type: 'GET',
            url: _BaseURL + '/' + _SystemLanguge + '/Home/CheckNafadStatus?transactionID=' + transactionID,
            success: function (data) {
                if (data.success) {
                    window.location.href = _BaseURL + '/' + _SystemLanguge + data.url;
                }
                $('#wait_overlay').hide();
            },
            error: function (er) {
                GeneralClass.AlertMessage(publicResources.ErrorOccurred, '', AlertsType.Warning);
            }
        });
    }
}