using AspNetCoreExample.Hubs;
using Microsoft.AspNetCore.SignalR;
using SiemensPlcConnection;
using SiemensPlcConnection.EventArgs;
using SiemensPlcConnection.Models;

namespace AspNetCoreExample.Services
{
    public class PlcListenerConfigurationService
    {
        private readonly IPlcConnectionFactory _connectionFactory;
        private readonly IHubContext<PlcConnectionHub> _hubContext;
        public PlcListenerConfigurationService(IPlcConnectionFactory plcConnectionFactory
            , IHubContext<PlcConnectionHub> hubContext)
        {
            _connectionFactory = plcConnectionFactory;
            _hubContext = hubContext;


        }

        public void CreateListener(
            Action<PlcConnectionConfiguration> config,
            string dbAddr,
            Action<OnValueChangedEventArgs> onValueChanged = null,
            Action<OnErrorEventArgs> onError = null,
            Action<ConnectionStatusChangedEventArgs> connectionStatusChanged = null)
        {
            var plcDevice = _connectionFactory.CreateInstance(config);

            if(onValueChanged != null)
                plcDevice.OnValueChanged += (s,e) => { onValueChanged.Invoke(e); };

            if (onError!= null)
                plcDevice.OnError += (s, e) => { onError.Invoke(e); };

            if (connectionStatusChanged != null)
                plcDevice.OnConnectionStatusChanged += (s, e) => { connectionStatusChanged.Invoke(e); };

            plcDevice.OnConnectionStatusChanged += (s, e) 
                => _hubContext.Clients.All.SendAsync("OnConnectionStatusChanged", e.IpAddress, e.ConnectionStatus);

            plcDevice.OnValueChanged += (s, e)
                => _hubContext.Clients.All.SendAsync("OnValueChanged", e.IpAddress, e.Variable, e.Value);

            plcDevice.OnError += (s, e)
               => _hubContext.Clients.All.SendAsync("OnError", e.IpAddress, e.Variable, e.Exception.Message);

            plcDevice.Connect();

            plcDevice.StartListening(dbAddr, 500, "");

        }


    }
}
