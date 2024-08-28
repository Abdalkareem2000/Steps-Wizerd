using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;

namespace DCCO.Presentation.DCCO.Application.Hub
{
    public class NafadHub : Microsoft.AspNetCore.SignalR.Hub
    {
        readonly ISessionService _sessionService;
        public NafadHub(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }
        private static ConcurrentDictionary<string, string> _connections =
            new ConcurrentDictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            //Context.Request.GetHttpContext().Session["IDNumber"].ToString()
            var transactionID = Context.GetHttpContext().Request.Query["TransactionID"].ToString();
            if (!string.IsNullOrEmpty(transactionID))
                _connections[transactionID] = Context.ConnectionId;
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var transactionID = _connections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (transactionID != null)
            { _connections.TryRemove(transactionID, out _); }
            return base.OnDisconnectedAsync(exception);
        }
        public async Task CheckNafadStatus(string transactionID)
        {
            if (_connections.TryGetValue(transactionID, out string connectionId))
                await Clients.Client(connectionId).SendAsync("ReceiveNafadStatus");
        }
    }
}