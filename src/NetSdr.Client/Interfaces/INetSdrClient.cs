using NetSdr.Client.Constants;

namespace NetSdr.Client.Interfaces;

public interface INetSdrClient : IDisposable
{
    bool IsConnected { get; }

    Task ConnectAsync(string host, int port = NetSdrDefaults.TcpPort,
        CancellationToken cancellationToken = default);

    Task DisconnectAsync(CancellationToken cancellationToken = default);

    Task StartIqTransferAsync(CancellationToken cancellationToken = default);

    Task StopIqTransferAsync(CancellationToken cancellationToken = default);

    Task SetFrequencyAsync(uint frequency, CancellationToken cancellationToken = default);
}
