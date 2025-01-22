using NetSdr.Client.Protocol;

namespace NetSdr.Client.Events;

public class UnsolicitedControlEventArgs : EventArgs
{
    public ControlItem ControlItem { get; }
    public ReadOnlyMemory<byte> Data { get; }

    public UnsolicitedControlEventArgs(ControlItem controlItem, ReadOnlyMemory<byte> data)
    {
        ControlItem = controlItem;
        Data = data;
    }
}
