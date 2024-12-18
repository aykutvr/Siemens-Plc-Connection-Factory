using S7.Net;
using SiemensPlcConnection.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensPlcConnection.EventArgs
{
    public class ConnectionStatusChangedEventArgs
    {
        public string IpAddress { get; }
        public CpuType Cpu { get; }
        public short Rack { get; }
        public short Slot { get; }
        public ConnectionStatus ConnectionStatus { get; }
        public ConnectionStatusChangedEventArgs(ConnectionStatus connectionStatus, string ipAddr, CpuType cpu, short rack, short slot)
        {
            ConnectionStatus = connectionStatus;
            IpAddress = ipAddr;
            Cpu = cpu;
            Rack = rack;
            Slot = slot;
        }

    }
}
