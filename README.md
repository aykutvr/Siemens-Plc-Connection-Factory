# Siemens PLC Connection Factory

This project provides a reusable and scalable connection factory structure for Siemens PLCs (Programmable Logic Controllers). The repository includes examples using **.NET Framework**, **.NET Core**, and **ASP.NET Core SignalR**, allowing you to explore how to integrate PLCs with different technologies.

## Features

- **Multi-Technology Support:** Examples built with .NET Framework, .NET Core, and ASP.NET Core SignalR  
- **Easy Integration:** Configure PLC connection details directly in code  
- **Modular Architecture:** Easily adaptable to different PLC types or communication protocols  
- **Scalability:** Connect to multiple PLCs simultaneously  
- **Maintainability:** Clean and organized codebase, making maintenance and development simpler and more efficient

## Project Structure

- **Siemens Plc Connection Interface:**  
  Focuses on a clear abstraction layer with:
  - **Interface-Driven Design:** Uses interfaces (e.g., `IPlcConnection`) to abstract PLC communication details.
  - **Configuration Objects:** Defines `PlcConnectionConfig` for PLC parameters.
  - **Factory Pattern:** Employs `SiemensPlcConnectionFactory` to create and manage PLC connections, making the system easily extensible.
  - **Clean Separation of Concerns:** Encapsulates low-level communication in connection classes.
  - **Scalability:** Facilitates adding new PLC types or protocols by adhering to defined interfaces.

## Getting Started

### Requirements

- .NET 6 or newer (specific examples may differ in requirements)  
- Siemens PLC connection details (IP, Rack, Slot)  
- Visual Studio, Visual Studio Code, or another suitable IDE/Editor

### Setup

1. Clone the repository:
    ```bash
    git clone https://github.com/aykutvr/Siemens-Plc-Connection-Factory.git
    ```
   
2. Navigate to the project directory:
    ```bash
    cd Siemens-Plc-Connection-Factory
    ```

3. Select the relevant example project and run `dotnet restore` to restore dependencies.

4. Build and run the application:
    ```bash
    dotnet build
    dotnet run
    ```

### Configuration

In console application you can use like following codes directly for creation new plc connection instance.

```csharp
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
```

In Asp.Net application you should use Dependency injection methods for factory implementation.

### Program.cs
```csharp
builder.Services.AddSingleton<IPlcConnectionFactory, PlcConnectionFactory>();
```

### Controller.cs
```csharp
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IPlcConnectionFactory _connectionFactory;
    private readonly IHubContext<PlcConnectionHub> _hubContext;

    public HomeController(ILogger<HomeController> logger
        ,IPlcConnectionFactory plcConnectionFactory
        , IHubContext<PlcConnectionHub> hubContext)
    {
        _logger = logger;
        _connectionFactory = plcConnectionFactory;
        _hubContext = hubContext;
    }

    public IActionResult Index()
    {
        var plcDevice = _connectionFactory.CreateInstance(config =>
        {
            config.Rack = 0;
            config.AutoConnect = false;
            config.AutoReconnect = false;
            config.CpuType = CpuType.S71500;
            config.IpAddress = "10.9.209.140";
        });

        plcDevice.OnConnectionStatusChanged += (s, e)
            => _hubContext.Clients.All.SendAsync("OnConnectionStatusChanged", e.IpAddress, e.ConnectionStatus);

        plcDevice.OnValueChanged += (s, e)
            => _hubContext.Clients.All.SendAsync("OnValueChanged", e.IpAddress, e.Variable, e.Value);

        plcDevice.OnError += (s, e)
           => _hubContext.Clients.All.SendAsync("OnError", e.IpAddress, e.Variable, e.Exception.Message);

        plcDevice.Connect();

        plcDevice.StartListening("DB300.DBX0.0", 500, "");


        return View();
    }
}
```
