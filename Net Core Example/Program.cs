// See https://aka.ms/new-console-template for more information
using S7.Net;
using SiemensPlcConnection;

Console.WriteLine("Hello, World!");

IPlcConnectionFactory plcFactory = new PlcConnectionFactory(multipleConnection: true);

//Create new plc device in factory
IPlcDevice plcDevice = plcFactory.CreateInstance(config =>
{
    config.Rack = 0;
    config.AutoConnect = false;
    config.AutoReconnect = false;
    config.CpuType = CpuType.S71500;
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
        Console.WriteLine($@"{e.IpAddress} {e.Variable} {e.Exception.ToString()}");
    };

    item.OnValueChanged += (sender, e) =>
    {
        Console.WriteLine($@"{e.IpAddress} {e.Variable} {e.Value} {e.Message}");
    };
}

//Start basically listening thread
plcFactory.Instances[0].StartListening("DB300.DBX0.0", delay: 500, message: "Default Listening Operation");

//Start listening thread with custom changed action
plcFactory.Instances[0].StartListening("DB300.DBX0.1"
    , delay: 500
    , message: "Datablock Listening Operation with Custom Changed Event"
    , onChangedAction: (e) =>
    {
        Console.WriteLine($@"{e.IpAddress} {e.Variable} {e.Value} {e.Message}");
    });

//Start listening thread with detailed datablock address
plcFactory.Instances[0].StartListening(DataType.DataBlock, 701, 72, VarType.Bit, 0, 0, 500);