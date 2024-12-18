using S7.Net;

namespace SiemensPlcConnection.EventArgs
{
    public class OnValueChangedEventArgs
    {
        public string Variable { get; }
        public string Message { get; }
        public object Value { get; set; }
        public string IpAddress { get; }
        public CpuType Cpu { get; }
        public short Rack { get; }
        public short Slot { get; }
        public OnValueChangedEventArgs(string variable, object value, string ipAddr, CpuType cpu, short rack, short slot, string message = "")
        {
            Variable = variable;
            Value = value;
            IpAddress = ipAddr;
            Cpu = cpu;
            Rack = rack;
            Slot = slot;
            Message = message;
        }
    }
}
