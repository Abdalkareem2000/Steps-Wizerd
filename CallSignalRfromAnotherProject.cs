await _nafadRepository.UpdateNafadLog(MapEnvelopeToNafadLog(nafadLog, envelope));

HubConnection _connection = new HubConnectionBuilder()
    .WithUrl(_config.GetValue<string>("HubUrl"))
    .Build();



await _connection.StartAsync();
await _connection.InvokeAsync("CheckNafadStatus", envelope.Header.RespHeader.TransactionId);
await _connection.StopAsync();