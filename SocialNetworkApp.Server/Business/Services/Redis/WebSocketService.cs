using System.Net.WebSockets;
using System.Text;

namespace SocialNetworkApp.Server.Business.Services.Redis
{
    public class WebSocketService(ICacheService cacheService)
    {
        
        private readonly ICacheService _redisService = cacheService;
       
        public async Task HandleWebSocketAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var chatId = context.Request.Query["chatId"]; // Implement this method to extract chatId from the request

                _redisService.Subscribe($"chat:{chatId}", async (channel, value) =>
                {
                    await SendMessageToClientAsync(webSocket, value.ToString());
                });

                // Keep the connection open until it is closed by the client
                await ReceiveMessagesAsync(webSocket);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task SendMessageToClientAsync(WebSocket webSocket, string message)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(buffer);
                await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }


        //ovo mozda i ne treba
        private async Task ReceiveMessagesAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            while (webSocket.State == WebSocketState.Open)
            {
                await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), 
                    CancellationToken.None);
            }
        }


    }   
}
