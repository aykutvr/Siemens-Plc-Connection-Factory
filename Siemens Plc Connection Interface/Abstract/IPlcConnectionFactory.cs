using SiemensPlcConnection.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensPlcConnection
{
    public interface IPlcConnectionFactory
    {
        bool MultipleInstance { get; }
        IPlcDevice MainInstance { get; }
        List<IPlcDevice> Instances { get; }
        IPlcDevice GetInstance(string ipAddr);
        IPlcDevice CreateInstance(Action<PlcConnectionConfiguration> config);
        void RemoveInstance(string ipAddr);

#if NET
        IPlcDevice this[int index] => Instances[index];
        IPlcDevice this[string ipAddress] => GetInstance(ipAddress);
#endif
    }
}
