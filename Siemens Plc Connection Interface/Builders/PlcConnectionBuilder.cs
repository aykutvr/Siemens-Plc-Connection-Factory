using S7.Net;
using SiemensPlcConnection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensPlcConnection.Builders
{
    public class PlcConnectionBuilder
    {
        public static IPlcDevice CreateConnection(string ip, CpuType cpuType, short rack, short slot)
        {
            return CreateConnection(config =>
            {
                config.IpAddress = ip;
                config.CpuType = cpuType;
                config.Rack = rack;
                config.Slot = slot;
            });
        }

        public static IPlcDevice CreateConnection(Action<PlcConnectionConfiguration> action)
        {
            var configValues = new PlcConnectionConfiguration();
            action.Invoke(configValues);

            return new PlcDevice(configValues);
        }


    }
}
