using S7.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensPlcConnection.Exceptions
{
    public class PlcConnectionException : PlcException
    {
        public string IpAddress { get; internal set; }
        public CpuType Cpu { get; internal set; }
        public short Rack { get; internal set; }
        public short Slot { get; internal set; }
        public string Variable { get; internal set; }
#if NETFRAMEWORK
        public object Value { get; internal set; }
#else
        public object Value { get; internal set; }
#endif
        public PlcConnectionException(ErrorCode errorCode) : base(errorCode)
        {
        }

        public PlcConnectionException(ErrorCode errorCode, Exception innerException) : base(errorCode, innerException)
        {
        }

        public PlcConnectionException(ErrorCode errorCode, string message) : base(errorCode, message)
        {
        }

        public PlcConnectionException(ErrorCode errorCode, string message, Exception inner) : base(errorCode, message, inner)
        {
        }
    }
}
