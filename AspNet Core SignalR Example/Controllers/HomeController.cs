using AspNetCoreExample.Hubs;
using AspNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using S7.Net;
using SiemensPlcConnection;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AspNetCoreExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPlcConnectionFactory _connectionFactory;
        private readonly IHubContext<PlcConnectionHub> _hubContext;

        public HomeController(ILogger<HomeController> logger
            ,IPlcConnectionFactory plcConnectionFactory
            , IHubContext<PlcConnectionHub> hubContext)
        {
            _logger = logger;
            _connectionFactory = plcConnectionFactory;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            var plcDevice = _connectionFactory.CreateInstance(config =>
            {
                config.Rack = 0;
                config.AutoConnect = false;
                config.AutoReconnect = false;
                config.CpuType = CpuType.S71500;
                config.IpAddress = "10.9.209.140";
            });

            plcDevice.OnConnectionStatusChanged += (s, e)
                => _hubContext.Clients.All.SendAsync("OnConnectionStatusChanged", e.IpAddress, e.ConnectionStatus);

            plcDevice.OnValueChanged += (s, e)
                => _hubContext.Clients.All.SendAsync("OnValueChanged", e.IpAddress, e.Variable, e.Value);

            plcDevice.OnError += (s, e)
               => _hubContext.Clients.All.SendAsync("OnError", e.IpAddress, e.Variable, e.Exception.Message);

            plcDevice.Connect();

            plcDevice.StartListening("DB300.DBX0.0", 500, "");


            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
