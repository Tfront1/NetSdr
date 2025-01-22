using Microsoft.Extensions.Logging;
using NetSdr.Client.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetSdr.Client.IqData;

public sealed class IqDataProcessor : IDisposable
{
    private readonly string _outputFilePath;
    private readonly UdpClient _udpClient;
    private readonly CancellationTokenSource _cts;
    private Task? _processingTask;
    private bool _isDisposed;
    private readonly ILogger? _logger;

    private const int HeaderSize = NetSdrDefaults.HeaderSize;
    private const int BufferSize = 65536;

    public IqDataProcessor(
        string outputFilePath,
        int port = NetSdrDefaults.UdpPort,
        ILogger? logger = null)
    {
        _outputFilePath = outputFilePath ?? throw new ArgumentNullException(nameof(outputFilePath));
        _udpClient = new UdpClient(port);
        _cts = new CancellationTokenSource();
        _logger = logger;
    }

    public void Start()
    {
        if (_processingTask != null)
        {
            throw new InvalidOperationException("IQ data processor is already running");
        }

        _processingTask = ProcessDataAsync();
    }

    private async Task ProcessDataAsync()
    {
        try
        {
            await using var fileStream = new FileStream(
                _outputFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            while (!_cts.Token.IsCancellationRequested)
            {
                var result = await _udpClient.ReceiveAsync(_cts.Token);

                if (result.Buffer.Length <= HeaderSize)
                {
                    _logger?.LogWarning("Received packet too small: {Length} bytes",
                        result.Buffer.Length);
                    continue;
                }

                try
                {
                    // Skip header and sequence number, write only I/Q data
                    await fileStream.WriteAsync(
                        result.Buffer.AsMemory(HeaderSize),
                        _cts.Token);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error writing IQ data to file");
                    throw;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
            _logger?.LogInformation("IQ data processing cancelled");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error processing IQ data");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_processingTask == null) return;

        try
        {
            _cts.Cancel();
            await _processingTask.WaitAsync(cancellationToken);
        }
        finally
        {
            _processingTask = null;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _cts.Cancel();
        _cts.Dispose();
        _udpClient.Dispose();

        _isDisposed = true;
    }
}
