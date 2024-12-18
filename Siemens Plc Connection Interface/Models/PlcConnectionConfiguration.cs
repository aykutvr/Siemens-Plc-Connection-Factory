using S7.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensPlcConnection.Models
{
    public class PlcConnectionConfiguration
    {
        public string IpAddress { get; set; } = string.Empty;
        public CpuType CpuType { get; set; }
        public short Rack { get; set; } = 0;
        public short Slot { get; set; } = 0;


        public bool AutoConnect { get; set; }
        public bool AutoReconnect { get; set; }

        public int ConnectionStateHandlingDelayTime { get; set; } = 100;

        internal string IsLiveBitAddr { get; set; } = string.Empty;
        internal bool IsLiveControlActive { get; set; } = false;
        public void IsLiveConfiguration(string dbAddr)
        {
            IsLiveControlActive = true;
            IsLiveBitAddr = dbAddr;
        }
    }
}
