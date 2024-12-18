namespace SiemensPlcConnection.EventArgs
{
    public class LiveBitChangedEventArgs
    {
        public bool Value { get; }
        public LiveBitChangedEventArgs(bool value)
        {
            Value = value;
        }
    }
}
