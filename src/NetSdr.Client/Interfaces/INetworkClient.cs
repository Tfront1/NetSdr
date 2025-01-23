namespace NetSdr.Client.Interfaces;

public interface INetworkClient
{
    bool IsConnected { get; }
    Task ConnectAsync(string host, int port, CancellationToken cancellationToken);
    Task WriteAsync(byte[] data, CancellationToken cancellationToken);
    Task<byte[]> ReadAsync(CancellationToken cancellationToken);
    void Disconnect();
}
