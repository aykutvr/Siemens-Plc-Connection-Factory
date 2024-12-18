using S7.Net;
using S7.Net.Types;

/* Unmerged change from project 'Siemens Plc Connection Interface (net471)'
Before:
using SiemensPlcConnection.Enums;
After:
using Siemens_Plc_Connection_Interface;
using SiemensPlcConnection.Enums;
*/
using SiemensPlcConnection;
using SiemensPlcConnection.Enums;
using SiemensPlcConnection.EventArgs;
using SiemensPlcConnection.Exceptions;
using SiemensPlcConnection.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiemensPlcConnection
{
    public class PlcDevice : IPlcDevice, IDisposable
    {
        #region External Properties


        public string IpAddress => ConnectionConfiguration.IpAddress;

        public CpuType Cpu => ConnectionConfiguration.CpuType;

        public short Rack => ConnectionConfiguration.Rack;

        public short Slot => ConnectionConfiguration.Slot;

        public bool AutoConnect => ConnectionConfiguration.AutoConnect;

        public bool AutoReconnect => ConnectionConfiguration.AutoReconnect;

        public ConnectionStatus ConnectionStatus => _connectionStatus;
        public bool IsConnected => _connectionStatus == ConnectionStatus.Connected;
        public PlcConnectionException LastException => _lastException;
        public ErrorCode LastErrorCode => _lastException == null ? ErrorCode.NoError : _lastException.ErrorCode;


        #endregion

        #region Internal Variables
        private PlcConnectionConfiguration ConnectionConfiguration { get; set; }
        private ConnectionStatus _connectionStatus { get; set; } = ConnectionStatus.Unknown;
        private bool _connectionStateListenerProcessIsRunning { get; set; } = false;
        private bool _liveBitListenerProcessIsRunning { get; set; } = false;
        private Plc _plc { get; set; }
        private CancellationTokenSource _ctsLiveBitListener;
        private CancellationTokenSource _ctsConnectionListener;
        private IDictionary<string, Task> _listeningVariables;
        private PlcConnectionException _lastException { get; set; } = null;
        #endregion

        #region Events
        public event EventHandler<ConnectionStatusChangedEventArgs> OnConnectionStatusChanged;
        public event EventHandler<LiveBitChangedEventArgs> OnLiveBitChanged;
        public event EventHandler<OnErrorEventArgs> OnError;
        public event EventHandler<OnValueChangedEventArgs> OnValueChanged;

        #endregion

        #region Ctor
        public PlcDevice(PlcConnectionConfiguration configuration)
        {
            ConnectionConfiguration = configuration;

            _ctsConnectionListener = new CancellationTokenSource();
            _ctsLiveBitListener = new CancellationTokenSource();

            ConfigAutoReconnect();

            ListenPlcConnectionStatus();

            CreateConnection();




            if (AutoConnect)
                Connect();


            ListenLiveBit();
        }
        #endregion

        #region External Methods
        public void Connect()
        {
            ChangeConnectionStatus(ConnectionStatus.Connecting);
            try
            {
                _plc.Open();
            }
            catch (PlcException ex)
            {
                ThrowException(new PlcConnectionException(ex.ErrorCode, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Rack = Rack, Slot = Slot }, ConnectionStatus.ConnectionError);
            }
            catch (Exception ex)
            {
                ThrowException(new PlcConnectionException(ErrorCode.ConnectionError, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Rack = Rack, Slot = Slot }, ConnectionStatus.ConnectionError);
            }

        }
        public void Disconnect()
        {
            try
            {
                _plc.Close();
            }
            catch (PlcException ex)
            {
                ThrowException(new PlcConnectionException(ex.ErrorCode, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Rack = Rack, Slot = Slot });
            }
            catch (Exception ex)
            {
                ThrowException(new PlcConnectionException(ErrorCode.ConnectionError, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Rack = Rack, Slot = Slot });
            }

            if (ConnectionStatus != ConnectionStatus.Disconnected)
                ChangeConnectionStatus(ConnectionStatus.Disconnected);
        }
        public void Reconnect()
        {
            Disconnect();
            CreateConnection();
            Connect();
        }
#if NETFRAMEWORK
        public object Read(string dbAddr)
#else
        public object Read(string dbAddr)
#endif
        {
            try
            {
                if (ConnectionStatus != ConnectionStatus.Connected)
                    throw new PlcConnectionException(ErrorCode.ReadData, "Plc device not connected") { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = dbAddr };

                return _plc.Read(dbAddr);
            }
            catch (PlcConnectionException ex)
            {
                ThrowException(ex);
            }
            catch (PlcException ex)
            {
                ThrowException(new PlcConnectionException(ex.ErrorCode, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = dbAddr });
            }
            catch (Exception ex)
            {
                ThrowException(new PlcConnectionException(ErrorCode.ReadData, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = dbAddr });
            }
            return null;
        }

#if NETFRAMEWORK
        public bool ReadAsBit(string dbAddr)
#else
        public bool? ReadAsBit(string dbAddr)
#endif
        {
            var result = Read(dbAddr);
            if (result == null)
                return false;
            else
                return Convert.ToBoolean(result);
        }

#if NETFRAMEWORK
        public int ReadAsInt(string dbAddr)
#else
        public int? ReadAsInt(string dbAddr)
#endif
        {
            var result = Read(dbAddr);
            if (result == null)
                return 0;
            else
                return Convert.ToInt32(result);
        }
#if NETFRAMEWORK
        public object Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0)
#else
        public object Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0)
#endif
        {
            try
            {
                if (ConnectionStatus != ConnectionStatus.Connected)
                    throw new PlcConnectionException(ErrorCode.ReadData, "Plc device not connected") { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = GetVariableString(dataType, db, startByteAdr, varType, varCount, bitAdr) };

                return _plc.Read(dataType, db, startByteAdr, varType, varCount, bitAdr);
            }
            catch (PlcConnectionException ex)
            {
                ThrowException(ex);
            }
            catch (PlcException ex)
            {
                ThrowException(new PlcConnectionException(ex.ErrorCode, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = GetVariableString(dataType, db, startByteAdr, varType, varCount, bitAdr) });
            }
            catch (Exception ex)
            {
                ThrowException(new PlcConnectionException(ErrorCode.ReadData, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = GetVariableString(dataType, db, startByteAdr, varType, varCount, bitAdr) });
            }
            return null;
        }
#if NETFRAMEWORK
        public object ReadStructure(Type structureType, int db, int startByteAdr = 0)
        {
            try
            {
                if (ConnectionStatus != ConnectionStatus.Connected)
                    throw new PlcConnectionException(ErrorCode.ReadData, "Plc device not connected") { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = db.ToString() };

                return _plc.ReadStruct(structureType, db, startByteAdr);
            }
            catch (PlcConnectionException ex)
            {
                ThrowException(ex);
            }
            catch (PlcException ex)
            {
                ThrowException(new PlcConnectionException(ex.ErrorCode, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = "" });
            }
            catch (Exception ex)
            {
                ThrowException(new PlcConnectionException(ErrorCode.ReadData, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = "" });
            }
            return null;
        }
#else
        public T? ReadStructure<T>(int db, int startByteAdr = 0)
                   where T : struct
        {
            try
            {
                if (ConnectionStatus != ConnectionStatus.Connected)
                    throw new PlcConnectionException(ErrorCode.ReadData, "Plc device not connected") { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = db.ToString() };

                return _plc.ReadStruct<T>(db, startByteAdr);
            }
            catch (PlcConnectionException ex)
            {
                ThrowException(ex);
            }
            catch (PlcException ex)
            {
                ThrowException(new PlcConnectionException(ex.ErrorCode, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = db.ToString() });
            }
            catch (Exception ex)
            {
                ThrowException(new PlcConnectionException(ErrorCode.ReadData, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = db.ToString() });
            }
            return null;
        }
#endif


        public ErrorCode Write(DataType dataType, int db, int startByteAdr, object value, int bitAdr = -1)
        {
            try
            {
                if (ConnectionStatus != ConnectionStatus.Connected)
                    throw new PlcConnectionException(ErrorCode.ConnectionError, "Plc device not connected") { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = GetVariableString(dataType, db, startByteAdr, VarType.Word, 0, Convert.ToByte(bitAdr < 0 ? 0 : bitAdr)) };

                _plc.Write(dataType, db, startByteAdr, value, bitAdr);
                return ErrorCode.NoError;
            }
            catch (PlcConnectionException ex)
            {
                ThrowException(ex);
                return ex.ErrorCode;
            }
            catch (PlcException ex)
            {
                ThrowException(new PlcConnectionException(ex.ErrorCode, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = GetVariableString(dataType, db, startByteAdr, VarType.Word, 0, Convert.ToByte(bitAdr < 0 ? 0 : bitAdr)) });
                return ex.ErrorCode;
            }
            catch (Exception ex)
            {
                ThrowException(new PlcConnectionException(ErrorCode.ConnectionError, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = GetVariableString(dataType, db, startByteAdr, VarType.Word, 0, Convert.ToByte(bitAdr < 0 ? 0 : bitAdr)) });
                return ErrorCode.WriteData;
            }
        }
        public ErrorCode Write(string dbAddr, object value)
        {
            try
            {
                if (ConnectionStatus != ConnectionStatus.Connected)
                    throw new PlcConnectionException(ErrorCode.ConnectionError, "Plc device not connected") { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = dbAddr };

                _plc.Write(dbAddr, value);
                return ErrorCode.NoError;
            }
            catch (PlcConnectionException ex)
            {
                ThrowException(ex);
                return ex.ErrorCode;
            }
            catch (PlcException ex)
            {
                ThrowException(new PlcConnectionException(ex.ErrorCode, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = dbAddr });
                return ex.ErrorCode;
            }
            catch (Exception ex)
            {
                ThrowException(new PlcConnectionException(ErrorCode.ConnectionError, ex.Message, ex) { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = dbAddr });
                return ErrorCode.WriteData;
            }
        }
        public void Dispose()
        {
            Disconnect();
            _plc = null;
            GC.Collect();
        }
        public void StartListening(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0, int delay = 500)
        {
            if (_listeningVariables == null)
                _listeningVariables = new Dictionary<string, Task>();
            string dbAddr = string.Format("{0}{1}{2}{3}{4}{5}", dataType, db, startByteAdr, varType, varCount, bitAdr);

            Task listeningTask = null;
            if (!_listeningVariables.TryGetValue(dbAddr, out listeningTask))
                _listeningVariables.Add(dbAddr, Task.Run(async () =>
                {
                    string lastValue = null;
                    while (true)
                    {
                        try
                        {
                            string newValue = Convert.ToString(Read(dataType, db, startByteAdr, varType, varCount, bitAdr));
                            if (newValue == null)
                                throw new Exception(ConnectionStatus.ToString());
                            if (lastValue != newValue)
                                OnValueChanged?.Invoke(this, new OnValueChangedEventArgs(dbAddr, lastValue = newValue, IpAddress, Cpu, Rack, Slot));
                        }
                        catch (Exception ex)
                        {
                            ThrowException(new PlcConnectionException(ErrorCode.ReadData, ex) { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = dbAddr });
                        }
                        finally
                        {
                            await Task.Delay(delay);
                        }
                    }
                }));
        }
        public void StartListening(string dbAddr, int delay = 500, string message = "")
        {
            StartListening(dbAddr, delay, message, null);
        }
        public void StartListening(string dbAddr, int delay = 500, string message = "", Action<OnValueChangedEventArgs> onChangedAction = null)
        {
            if (_listeningVariables == null)
                _listeningVariables = new Dictionary<string, Task>();

            Task listeningTask = null;
            if (!_listeningVariables.TryGetValue(dbAddr, out listeningTask))
                _listeningVariables.Add(dbAddr, Task.Run(async () =>
                {
                    string lastValue = null;
                    while (true)
                    {
                        try
                        {
                            if (ConnectionStatus == ConnectionStatus.Connected)
                            {
                                object result = Read(dbAddr);
                                string newValue = Convert.ToString(result);
                                if (newValue == null)
                                    throw new Exception(ConnectionStatus.ToString());
                                if (lastValue != newValue)
                                {
                                    lastValue = newValue;
                                    OnValueChanged?.Invoke(this, new OnValueChangedEventArgs(dbAddr, result, IpAddress, Cpu, Rack, Slot, message));
                                    onChangedAction?.Invoke(new OnValueChangedEventArgs(dbAddr, result, IpAddress, Cpu, Rack, Slot, message));
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            ThrowException(new PlcConnectionException(ErrorCode.ReadData, ex) { Cpu = Cpu, IpAddress = IpAddress, Slot = Slot, Rack = Rack, Variable = dbAddr });
                        }
                        finally
                        {
                            await Task.Delay(delay);
                        }
                    }
                }));
        }
        public void StopListening(string dbAddr)
        {
            if (!_listeningVariables.ContainsKey(dbAddr))
                return;

            _listeningVariables[dbAddr].Dispose();
            _listeningVariables.Remove(dbAddr);
        }
        public void StopListening(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0)
        {
            string dbAddr = string.Format("{0}{1}{2}{3}{4}{5}", dataType, db, startByteAdr, varType, varCount, bitAdr);
            if (!_listeningVariables.ContainsKey(dbAddr))
                return;

            _listeningVariables[dbAddr].Dispose();
            _listeningVariables.Remove(dbAddr);
        }
        #endregion

        #region Internal Methods
        private void CreateConnection()
        {


            _plc = new Plc(Cpu, IpAddress, Rack, Slot);
            ChangeConnectionStatus(ConnectionStatus.ReadyForConnection);
        }
        private void ListenPlcConnectionStatus()
        {
            if (_connectionStateListenerProcessIsRunning)
                return;

            _connectionStateListenerProcessIsRunning = true;
            Task.Run(() =>
            {
                bool oldStatus = false;
                while (!_ctsConnectionListener.IsCancellationRequested)
                {
                    if (_plc != null && oldStatus != _plc.IsConnected)
                        ChangeConnectionStatus(oldStatus = _plc.IsConnected);

                    if (_plc != null && !_plc.IsConnected && AutoReconnect && ConnectionStatus == ConnectionStatus.ConnectionError)
                    {
                        Reconnect();
                    }

                    Task.Delay(ConnectionConfiguration.ConnectionStateHandlingDelayTime).Wait();
                }
                _connectionStateListenerProcessIsRunning = false;
            });
        }
        private void ChangeConnectionStatus(ConnectionStatus status)
        {
            _connectionStatus = status;
            if (OnConnectionStatusChanged == null)
                return;

            OnConnectionStatusChanged.Invoke(this, new ConnectionStatusChangedEventArgs(status, IpAddress, Cpu, Rack, Slot));
        }
        private void ChangeConnectionStatus(bool isConnected)
        {
            ChangeConnectionStatus(isConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected);
        }
        private void ChangeLiveBitStatus(bool liveBitValue)
        {
            if (OnLiveBitChanged == null)
                return;

            OnLiveBitChanged.Invoke(this, new LiveBitChangedEventArgs(liveBitValue));
        }
        private void ListenLiveBit()
        {
            if (!ConnectionConfiguration.IsLiveControlActive)
                return;

            if (_liveBitListenerProcessIsRunning)
                return;

            _liveBitListenerProcessIsRunning = true;

            Task.Run(() =>
            {
                bool oldValue = false;
                while (!_ctsLiveBitListener.IsCancellationRequested)
                {
                    if (ConnectionStatus == ConnectionStatus.Connected)
                    {
                        var value = Convert.ToBoolean(Read(ConnectionConfiguration.IsLiveBitAddr));
                        if (value != oldValue)
                            ChangeLiveBitStatus(oldValue = value);
                    }
                    Task.Delay(500).Wait();
                }
                _liveBitListenerProcessIsRunning = false;
            });
        }
        private void ThrowException(PlcConnectionException ex, ConnectionStatus? stt = null)
        {
            //if (OnError == null)
            //    throw ex;

            _lastException = ex;

            OnError?.Invoke(this, new OnErrorEventArgs(ex, IpAddress, Cpu, Rack, Slot, ex.Variable));

            if (stt != null)
                ChangeConnectionStatus(stt.Value);
        }
        private string GetVariableString(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAdr = 0)
        {
            StringBuilder variable = new StringBuilder();
            switch (dataType)
            {
                case DataType.Input:
                    break;
                case DataType.Output:
                    break;
                case DataType.Memory:
                    break;
                case DataType.DataBlock:
                    variable.Append("DB");
                    break;
                case DataType.Timer:
                    break;
                case DataType.Counter:
                    break;
                default:
                    break;
            }

            variable.Append(db.ToString());
            variable.Append(".");
            switch (varType)
            {
                case VarType.Bit:
                    variable.Append("DBX");
                    break;
                case VarType.Byte:
                    variable.Append("DBB");
                    break;
                case VarType.Word:
                    variable.Append("DBW");
                    break;
                case VarType.DWord:
                    variable.Append("DBW");
                    break;
                case VarType.Int:
                    variable.Append("DBD");
                    break;
                case VarType.DInt:
                    variable.Append("DBD");
                    break;
                case VarType.Real:
                    variable.Append("DBD");
                    break;
                case VarType.LReal:
                    variable.Append("DBD");
                    break;
                case VarType.String:
                    variable.Append("DBW");
                    break;
                case VarType.S7String:
                    variable.Append("DBW");
                    break;
                case VarType.S7WString:
                    variable.Append("DBW");
                    break;
                case VarType.Timer:
                    break;
                case VarType.Counter:
                    break;
                case VarType.DateTime:
                    break;
                case VarType.DateTimeLong:
                    break;
                default:
                    break;
            }

            variable.Append(startByteAdr);
            if (varType == VarType.Bit)
            {
                variable.Append(".");
                variable.Append(bitAdr);
            }
            else
            {
                variable.Append(" ");
                variable.Append(varCount);
            }
            return variable.ToString();
        }
        private void ConfigAutoReconnect()
        {
            if (AutoReconnect)
                OnConnectionStatusChanged += (s, e) =>
                {
                    if (e.ConnectionStatus == ConnectionStatus.Disconnected)
                        Reconnect();
                };
        }





        #endregion

    }
}
