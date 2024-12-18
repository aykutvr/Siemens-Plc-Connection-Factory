using S7.Net;
using SiemensPlcConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFrameworkExample
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //Create Plc Device Factory
            SiemensPlcConnection.IPlcConnectionFactory plcFactory = new SiemensPlcConnection.PlcConnectionFactory(multipleConnection: true);

            //Create new plc device in factory
            IPlcDevice plcDevice = plcFactory.CreateInstance(config =>
            {
                config.Rack = 0;
                config.AutoConnect = false;
                config.AutoReconnect = false;
                config.CpuType = CpuType.S71200;
                config.IpAddress = "10.9.209.140";
            });

            //Generate events for plc devices
            foreach (var item in plcFactory.Instances)
            {
                item.OnConnectionStatusChanged += (sender, e) =>
                {
                    Console.WriteLine($@"{e.IpAddress} {e.ConnectionStatus}");
                };

                item.OnError += (sender, e) =>
                {
                    Console.WriteLine($@"{e.IpAddress} {e.Variable} {e.Exception.Message.ToString()}");
                };

                item.OnValueChanged += (sender, e) =>
                {
                    Console.WriteLine($@"{e.IpAddress} {e.Variable} {e.Value} {e.Message}");
                };
            }

            //Manually connect to the plc
            plcFactory.Instances[0].Connect();

            //Start basically listening thread
            plcFactory.Instances[0].StartListening("DB300.DBX0.0", delay: 2000, message: "Default Listening Operation");

            //Start listening thread with custom changed action
            plcFactory.Instances[0].StartListening("DB300.DBX0.1"
                , delay: 2000
                , message: "Datablock Listening Operation with Custom Changed Event"
                , onChangedAction: (e) =>
                {
                    Console.WriteLine($@"{e.IpAddress} {e.Variable} {e.Value} {e.Message}");
                });

            //Start listening thread with detailed datablock address
            plcFactory.Instances[0].StartListening(DataType.DataBlock, 300, 0, VarType.Bit, 0, 0, 2000);

            Console.ReadKey();
        }
    }
}
