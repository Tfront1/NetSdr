using Microsoft.Extensions.Logging;
using NetSdr.Client.Commands;
using NetSdr.Client.Constants;
using NetSdr.Client.Events;
using NetSdr.Client.Exceptions;
using NetSdr.Client.Interfaces;
using NetSdr.Client.IqData;
using NetSdr.Client.Protocol;

namespace NetSdr.Client;

public class NetSdrClient : INetSdrClient
{
    private readonly INetworkClient _networkClient;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IqDataProcessor? _iqProcessor;
    private readonly ILogger? _logger;
    private bool _isDisposed;

    public bool IsConnected => _networkClient.IsConnected;

    public event EventHandler<UnsolicitedControlEventArgs>? UnsolicitedControlReceived;

    public NetSdrClient(INetworkClient networkClient, ILogger? logger = null)
    {
        _networkClient = networkClient;
        _logger = logger;
    }

    public async Task ConnectAsync(string host, int port = NetSdrDefaults.TcpPort, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(host))
            throw new ArgumentNullException(nameof(host));

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (IsConnected)
                throw new NetSdrClientException("Already connected");

            _logger?.LogInformation("Connecting to {Host}:{Port}", host, port);
            await _networkClient.ConnectAsync(host, port, cancellationToken);
            _logger?.LogInformation("Connected successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Connection failed");
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_iqProcessor != null)
            {
                await _iqProcessor.StopAsync(cancellationToken);
                _iqProcessor.Dispose();
                _iqProcessor = null;
            }

            _networkClient.Disconnect();
            _logger?.LogInformation("Disconnected");
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
            await _networkClient.WriteAsync(command, cancellationToken);
            var response = await _networkClient.ReadAsync(cancellationToken);

            if (response.Length > 0)
            {
                if (NetSdrProtocol.IsNakMessage(response))
                {
                    throw new NetSdrClientException("Command not supported");
                }

                if (MessageParser.TryParseMessage(response, out var header, out var controlItem, out var parameters) &&
                    header.Type == MessageType.UnsolicitedControlItem)
                {
                    OnUnsolicitedControl(controlItem!.Value, parameters);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new NetSdrClientException("Command failed", ex);
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