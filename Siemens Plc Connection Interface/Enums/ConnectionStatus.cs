using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensPlcConnection.Enums
{
    public enum ConnectionStatus
    {
        Unknown = 0,
        ReadyForConnection = 1,
        Connecting = 2,
        Connected = 3,
        Disconnected = 4,
        ConnectionError = 5
    }

}
