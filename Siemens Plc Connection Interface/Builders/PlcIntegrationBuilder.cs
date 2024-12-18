using Microsoft.Extensions.DependencyInjection;
using S7.Net;
using SiemensPlcConnection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SiemensPlcConnection.Builders
{
    public class PlcIntegrationBuilder
    {
        internal bool _multiplePlcConnectionModeOn = false;
        internal List<Action<PlcConnectionConfiguration>> _connections;
        internal IServiceCollection _services;

        public PlcIntegrationBuilder(IServiceCollection services)
        {
            _services = services;
        }


        public PlcIntegrationBuilder MultipleConnectionMode()
        {
            _multiplePlcConnectionModeOn = true;
            return this;
        }

        public PlcIntegrationBuilder AddPlc(Action<PlcConnectionConfiguration> config)
        {
            if (_connections == null)
                _connections = new List<Action<PlcConnectionConfiguration>>();

            _connections.Add(config);

            if (_connections.Count > 1)
                _multiplePlcConnectionModeOn = true;

            return this;
        }
        public PlcIntegrationBuilder AddPlc(string ipAddress, CpuType cpu, short rack, short slot, bool autoConnect = true, bool autoReconnect = true)
        {
            AddPlc(config =>
            {
                config.IpAddress = ipAddress;
                config.CpuType = cpu;
                config.Rack = rack;
                config.Slot = slot;
                config.AutoConnect = autoConnect;
                config.AutoReconnect = autoReconnect;
            });
            return this;
        }
        internal void Build(IServiceCollection services)
        {
            if (_multiplePlcConnectionModeOn)
            {
                IPlcConnectionFactory factory = new PlcConnectionFactory(_multiplePlcConnectionModeOn);
                foreach (var item in _connections)
                    factory.CreateInstance(item);
            }
        }
    }
}
