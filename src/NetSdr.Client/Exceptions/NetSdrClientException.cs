using NetSdr.Client.Protocol;

namespace NetSdr.Client.Exceptions;

public class NetSdrClientException : NetSdrException
{
    public ControlItem? ControlItem { get; }

    public NetSdrClientException(string message) : base(message) { }

    public NetSdrClientException(string message, ControlItem controlItem)
        : base(message)
    {
        ControlItem = controlItem;
    }

    public NetSdrClientException(string message, Exception innerException)
        : base(message, innerException) { }

    public NetSdrClientException(string message, ControlItem controlItem, Exception innerException)
        : base(message, innerException)
    {
        ControlItem = controlItem;
    }
}
