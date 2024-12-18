using S7.Net;
using SiemensPlcConnection.Enums;
using SiemensPlcConnection.EventArgs;
using SiemensPlcConnection.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensPlcConnection
{
    public interface IPlcDevice : IDisposable
    {
        #region Properties
        string IpAddress { get; }
        CpuType Cpu { get; }
        short Rack { get; }
        short Slot { get; }
        bool AutoConnect { get; }
        bool AutoReconnect { get; }
        ConnectionStatus ConnectionStatus { get; }
        ErrorCode LastErrorCode { get; }
        bool IsConnected { get; }
        PlcConnectionException LastException { get; }
        #endregion

        #region Events

        /* Unmerged change from project 'Siemens Plc Connection Interface (net7.0)'
        Before:
                event EventHandler<EventArgs.ConnectionStatusChangedEventArgs> OnConnectionStatusChanged;
        After:
                event EventHandler<ConnectionStatusChangedEventArgs> OnConnectionStatusChanged;
        */
        event EventHandler<ConnectionStatusChangedEventArgs> OnConnectionStatusChanged;

        /* Unmerged change from project 'Siemens Plc Connection Interface (net7.0)'
        Before:
                event EventHandler<EventArgs.LiveBitChangedEventArgs> OnLiveBitChanged;
        After:
                event EventHandler<LiveBitChangedEventArgs> OnLiveBitChanged;
        */
        event EventHandler<LiveBitChangedEventArgs> OnLiveBitChanged;
        event EventHandler<OnErrorEventArgs> OnError;
        event EventHandler<OnValueChangedEventArgs> OnValueChanged;
        #endregion

        void Connect();
        void Disconnect();
        void Reconnect();
#if NETFRAMEWORK
        object Read(string dbAddr);
        bool ReadAsBit(string dbAddr);
        int ReadAsInt(string dbAddr);
        object Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0);
#else
        object Read(string dbAddr);
        bool? ReadAsBit(string dbAddr);
        int? ReadAsInt(string dbAddr);
        object Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0);
#endif

        ErrorCode Write(string dbAddr, object value);
        ErrorCode Write(DataType dataType, int db, int startByteAdr, object value, int bitAdr = -1);
        void StartListening(string dbAddr, int delay = 500, string message = "");
        void StartListening(string dbAddr, int delay = 500, string message = "", Action<OnValueChangedEventArgs> onChangedAction = null);
        void StartListening(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0, int delay = 500);
        void StopListening(string dbAddr);
        void StopListening(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0);
#if NETFRAMEWORK
        object ReadStructure(Type structureType, int db, int startByteAdr = 0);
#else
        T? ReadStructure<T>(int db, int startByteAdr = 0) where T : struct;
#endif
    }


}
