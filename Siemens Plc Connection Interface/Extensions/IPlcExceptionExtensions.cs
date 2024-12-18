using SiemensPlcConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemens_Plc_Connection_Interface
{
    public static partial class Extensions
    {
        public static bool ReadBit(this IPlcDevice @this, string variable)
        {
            return Convert.ToBoolean(@this.Read(variable));
        }
    }
}
