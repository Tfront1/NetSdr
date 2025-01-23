using BenchmarkDotNet.Attributes;
using Moq;
using NetSdr.Client;
using NetSdr.Client.Commands;
using NetSdr.Client.Interfaces;

namespace NetSdr.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class NetSdrClientBenchmarks
{
    private Mock<INetworkClient> _networkClient;
    private NetSdrClient _client;
    private byte[] _testCommand;

    [GlobalSetup]
    public void Setup()
    {
        _networkClient = new Mock<INetworkClient>();
        _networkClient.Setup(x => x.IsConnected).Returns(true);
        _networkClient.Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _networkClient.Setup(x => x.ReadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        _client = new NetSdrClient(_networkClient.Object);
        _testCommand = ReceiverCommands.CreateSetFrequencyCommand(14_100_000);
    }

    [Benchmark]
    public async Task SendCommand() =>
        await _client.SetFrequencyAsync(14_100_000);

    [Benchmark]
    public async Task StartStopIqTransfer()
    {
        await _client.StartIqTransferAsync();
        await _client.StopIqTransferAsync();
    }

    [Benchmark]
    public async Task CompleteWorkflow()
    {
        await _client.ConnectAsync("localhost");
        await _client.SetFrequencyAsync(14_100_000);
        await _client.StartIqTransferAsync();
        await _client.StopIqTransferAsync();
        await _client.DisconnectAsync();
    }
}
