using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiemensPlcConnection;
using SiemensPlcConnection.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemens_Plc_Connection_Interface
{
    public static partial class Extensions
    {
        public static IServiceCollection AddPlcConnection(this IServiceCollection @this, Action<PlcIntegrationBuilder> builder)
        {
            var intBuilder = new PlcIntegrationBuilder(@this);
            builder.Invoke(intBuilder);

            IPlcConnectionFactory factory = new PlcConnectionFactory(intBuilder._multiplePlcConnectionModeOn);
            if (intBuilder._connections.Any())
                foreach (var item in intBuilder._connections)
                    factory.CreateInstance(item);

            @this.AddSingleton<IPlcConnectionFactory>(factory);

            if (intBuilder._connections.Any())
                @this.AddSingleton<IPlcDevice>(factory.MainInstance);



            return @this;

        }
    }
}
