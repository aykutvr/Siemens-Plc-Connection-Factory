using Microsoft.AspNetCore.SignalR;

namespace AspNetCoreExample.Hubs
{
    public class PlcConnectionHub : Hub
    {
        private static int connectionCount = 0;
        public override Task OnConnectedAsync()
        {
            connectionCount++;
            Console.WriteLine($@"Active Connection Count = {connectionCount}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            connectionCount--;
            Console.WriteLine($@"Active Connection Count = {connectionCount}");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
