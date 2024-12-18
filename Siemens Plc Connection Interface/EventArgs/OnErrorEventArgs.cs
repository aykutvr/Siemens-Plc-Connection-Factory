using S7.Net;
using SiemensPlcConnection.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensPlcConnection.EventArgs
{
    public class OnErrorEventArgs
    {
        public OnErrorEventArgs(PlcConnectionException exception, string ipAddr, CpuType cpu, short rack, short slot, string variable)
        {
            Exception = exception;
            IpAddress = ipAddr;
            Cpu = cpu;
            Rack = rack;
            Slot = slot;
            Variable = variable;
        }

        public PlcConnectionException Exception { get; set; }
        public string IpAddress { get; }
        public CpuType Cpu { get; }
        public short Rack { get; }
        public short Slot { get; }
        public string Variable { get; }
    }
}
