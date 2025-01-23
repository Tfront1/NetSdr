namespace NetSdr.Client.Protocol;

/// <summary>
/// Control item codes defined by the NetSDR protocol specification
/// </summary>
public enum ControlItem : ushort
{
    ReceiverState = 0x0018,
    ReceiverFrequency = 0x0020,
    RfFilter = 0x0044,
    AdModes = 0x008A,
    IqOutputDataSampleRate = 0x00B8
}