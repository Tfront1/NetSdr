using NetSdr.Client.Interfaces;
using System.Net.Sockets;

namespace NetSdr.Client;

/// <summary>
/// Implements TCP network communication for NetSDR protocol
/// </summary>
public class NetworkClient : INetworkClient
{
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;

    public bool IsConnected => _tcpClient?.Connected ?? false;

    /// <summary>
    /// Establishes TCP connection to specified host and port
    /// </summary>
    public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken)
    {
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(host, port, cancellationToken);
        _stream = _tcpClient.GetStream();
    }

    /// <summary>
    /// Writes data to TCP stream
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when not connected</exception>
    public async Task WriteAsync(byte[] data, CancellationToken cancellationToken)
    {
        if (_stream == null) throw new InvalidOperationException("Not connected");
        await _stream.WriteAsync(data, cancellationToken);
    }

    /// <summary>
    /// Reads response data from TCP stream
    /// </summary>
    /// <returns>Read bytes or empty array if no data available</returns>
    /// <exception cref="InvalidOperationException">Thrown when not connected</exception>
    public async Task<byte[]> ReadAsync(CancellationToken cancellationToken)
    {
        if (_stream == null) throw new InvalidOperationException("Not connected");
        var buffer = new byte[1024];
        var bytesRead = await _stream.ReadAsync(buffer, cancellationToken);
        return bytesRead > 0 ? buffer[..bytesRead] : Array.Empty<byte>();
    }

    /// <summary>
    /// Closes connection and cleans up resources
    /// </summary>
    public void Disconnect()
    {
        _stream?.Dispose();
        _tcpClient?.Dispose();
        _stream = null;
        _tcpClient = null;
    }
}
