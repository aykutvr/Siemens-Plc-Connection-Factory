using SiemensPlcConnection.Builders;
using SiemensPlcConnection.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensPlcConnection
{
    public class PlcConnectionFactory : IPlcConnectionFactory
    {

        public bool MultipleInstance { get => _multipleInstance; }

        public IPlcDevice MainInstance { get => Instances.Any() ? Instances.First() : null; }

        public List<IPlcDevice> Instances { get; private set; } = new List<IPlcDevice>();

        private bool _multipleInstance;


        public PlcConnectionFactory()
        {
        }
        public PlcConnectionFactory(bool multipleConnection)
        {
            _multipleInstance = multipleConnection;
        }
        public IPlcDevice CreateInstance(Action<PlcConnectionConfiguration> config)
        {
            PlcConnectionConfiguration instanceConfiguration = new PlcConnectionConfiguration();
            config.Invoke(instanceConfiguration);

            if (!Instances.Any(a => a.IpAddress == instanceConfiguration.IpAddress))
                Instances.Add(PlcConnectionBuilder.CreateConnection(config));

            return Instances.First(f => f.IpAddress == instanceConfiguration.IpAddress);
        }
        public IPlcDevice GetInstance(string ipAddr)
        {
            if (!Instances.Any(a => a.IpAddress == ipAddr))
                return null;

            return Instances.First(f => f.IpAddress == ipAddr);
        }
        public void RemoveInstance(string ipAddr)
        {
            Instances.RemoveAll(w => w.IpAddress == ipAddr);
        }
    }
}
