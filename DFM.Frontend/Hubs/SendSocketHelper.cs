using DFM.Shared.DTOs;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace DFM.Frontend.Hubs
{
    public class SendSocketHelper : ISendSocketHelper
    {
        private readonly IHubContext<NotifyHub> hubContext;

        public SendSocketHelper(IHubContext<NotifyHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public async Task SendThroughSocket(SocketSendModel model)
        {
            await hubContext.Clients.All.SendAsync("ReceiveMessage", JsonSerializer.Serialize(model.Topic), model.Message);
        }
    }

    public interface ISendSocketHelper
    {
        Task SendThroughSocket(SocketSendModel model);
    }
}
