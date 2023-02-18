using Microsoft.AspNetCore.SignalR;

namespace DFM.Frontend.Hubs
{
    public class NotifyHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
