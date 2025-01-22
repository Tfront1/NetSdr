namespace NetSdr.Client.Exceptions;

public class NetSdrException : Exception
{
    public NetSdrException() : base() { }

    public NetSdrException(string message) : base(message) { }

    public NetSdrException(string message, Exception innerException)
        : base(message, innerException) { }
}
