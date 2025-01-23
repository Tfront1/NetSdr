using Microsoft.Extensions.Logging;
using NetSdr.Client.Commands;
using NetSdr.Client.Constants;
using NetSdr.Client.Events;
using NetSdr.Client.Exceptions;
using NetSdr.Client.Interfaces;
using NetSdr.Client.IqData;
using NetSdr.Client.Protocol;
using System.Net.Sockets;

namespace NetSdr.Client;

public class NetSdrClient : INetSdrClient
{
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private IqDataProcessor? _iqProcessor;
    private readonly ILogger? _logger;
    private bool _isDisposed;

    public bool IsConnected => _tcpClient?.Connected ?? false;

    public event EventHandler<UnsolicitedControlEventArgs>? UnsolicitedControlReceived;

    public NetSdrClient(ILogger? logger = null)
    {
        _logger = logger;
    }

    public async Task ConnectAsync(
        string host,
        int port = NetSdrDefaults.TcpPort,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(host))
            throw new ArgumentNullException(nameof(host));

        await _connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsConnected)
                throw new NetSdrClientException("Already connected to device");

            _logger?.LogInformation("Connecting to {Host}:{Port}", host, port);

            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);
            _stream = _tcpClient.GetStream();

            _logger?.LogInformation("Successfully connected to {Host}:{Port}", host, port);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to connect to {Host}:{Port}", host, port);
            _tcpClient?.Dispose();
            _tcpClient = null;
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_iqProcessor != null)
            {
                await _iqProcessor.StopAsync(cancellationToken).ConfigureAwait(false);
                _iqProcessor.Dispose();
                _iqProcessor = null;
            }

            _stream?.Dispose();
            _tcpClient?.Dispose();
            _stream = null;
            _tcpClient = null;

            _logger?.LogInformation("Disconnected from device");
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task StartIqTransferAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfNotConnected();

        try
        {
            _logger?.LogInformation("Starting IQ transfer");

            var command = ReceiverCommands.CreateStartIqTransferCommand();
            await SendCommandAsync(command, cancellationToken).ConfigureAwait(false);

            // Initialize and start IQ data processor if not already running
            if (_iqProcessor == null)
            {
                _iqProcessor = new IqDataProcessor("iq_data.bin", NetSdrDefaults.UdpPort, _logger);
                _iqProcessor.Start();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to start IQ transfer");
            throw;
        }
    }

    public async Task StopIqTransferAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfNotConnected();

        try
        {
            _logger?.LogInformation("Stopping IQ transfer");

            var command = ReceiverCommands.CreateStopIqTransferCommand();
            await SendCommandAsync(command, cancellationToken).ConfigureAwait(false);

            if (_iqProcessor != null)
            {
                await _iqProcessor.StopAsync(cancellationToken).ConfigureAwait(false);
                _iqProcessor.Dispose();
                _iqProcessor = null;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to stop IQ transfer");
            throw;
        }
    }

    public async Task SetFrequencyAsync(uint frequency, CancellationToken cancellationToken = default)
    {
        ThrowIfNotConnected();

        try
        {
            _logger?.LogInformation("Setting frequency to {Frequency} Hz", frequency);

            var command = ReceiverCommands.CreateSetFrequencyCommand(frequency);
            await SendCommandAsync(command, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to set frequency to {Frequency} Hz", frequency);
            throw;
        }
    }

    private async Task SendCommandAsync(byte[] command, CancellationToken cancellationToken)
    {
        ThrowIfNotConnected();

        try
        {
            await _stream!.WriteAsync(command, cancellationToken).ConfigureAwait(false);

            // Read response
            var buffer = new byte[NetSdrDefaults.DefaultBufferSize];
            var bytesRead = await _stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

            if (bytesRead > 0)
            {
                // Створюємо новий масив з отриманими даними
                var responseData = new byte[bytesRead];
                Array.Copy(buffer, 0, responseData, 0, bytesRead);

                if (NetSdrProtocol.IsNakMessage(responseData))
                {
                    _logger?.LogWarning("Received NAK response from device");
                    throw new NetSdrClientException("Command not supported by device");
                }

                // Check for unsolicited control message
                MessageHeader header;
                ControlItem? controlItem;
                byte[] parameters;

                if (MessageParser.TryParseMessage(responseData, out header, out controlItem, out parameters) &&
                    header.Type == MessageType.UnsolicitedControlItem)
                {
                    OnUnsolicitedControl(controlItem!.Value, parameters);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger?.LogError(ex, "Error sending command to device");
            throw new NetSdrClientException("Failed to send command to device", ex);
        }
    }

    private void ThrowIfNotConnected()
    {
        if (!IsConnected)
            throw new NetSdrClientException("Not connected to device");
    }

    protected virtual void OnUnsolicitedControl(ControlItem controlItem, byte[] data)
    {
        UnsolicitedControlReceived?.Invoke(this,
            new UnsolicitedControlEventArgs(controlItem, data));
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        try
        {
            DisconnectAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // Ignore disposal errors
        }

        _connectionLock.Dispose();
        _isDisposed = true;

        GC.SuppressFinalize(this);
    }
}