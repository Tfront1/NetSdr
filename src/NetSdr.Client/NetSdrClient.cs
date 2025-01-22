using NetSdr.Client.Interfaces;

namespace NetSdr.Client;

public class NetSdrClient : INetSdrClient
{
    public bool IsConnected => throw new NotImplementedException();

    public Task ConnectAsync(string host, int port = 50000, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task SetFrequencyAsync(uint frequency, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task StartIqTransferAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task StopIqTransferAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
