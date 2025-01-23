using Microsoft.Extensions.Logging;
using Moq;
using NetSdr.Client.Exceptions;
using NetSdr.Client.Interfaces;

namespace NetSdr.Client.Tests;

[TestFixture]
public class NetSdrClientTests : IDisposable
{
    private Mock<INetworkClient> _networkClient;
    private Mock<ILogger> _logger;
    private NetSdrClient _sut;

    [SetUp]
    public void Setup()
    {
        _networkClient = new Mock<INetworkClient>();
        _logger = new Mock<ILogger>();
        _sut = new NetSdrClient(_networkClient.Object, _logger.Object);
        _networkClient.Setup(x => x.IsConnected).Returns(true);
        SetupDefaultResponses();
    }

    private void SetupDefaultResponses()
    {
        _networkClient
            .Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _networkClient
            .Setup(x => x.ReadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());
    }

    [Test]
    public async Task Connect_WhenNotConnected_Succeeds()
    {
        _networkClient.Setup(x => x.IsConnected).Returns(false);
        await _sut.ConnectAsync("localhost");
        _networkClient.Verify(x => x.ConnectAsync("localhost", 50000, It.IsAny<CancellationToken>()));
    }

    [Test]
    public void Connect_WhenAlreadyConnected_ThrowsException()
    {
        Assert.ThrowsAsync<NetSdrClientException>(() => _sut.ConnectAsync("localhost"));
    }

    [Test]
    public async Task SetFrequency_SendsCorrectCommand()
    {
        byte[] capturedCommand = null;
        _networkClient
            .Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Callback<byte[], CancellationToken>((cmd, _) => capturedCommand = cmd)
            .Returns(Task.CompletedTask);

        await _sut.SetFrequencyAsync(14_100_000);

        var frequencyBytes = new byte[4];
        Array.Copy(capturedCommand, 5, frequencyBytes, 0, 4);
        Assert.That(BitConverter.ToUInt32(frequencyBytes), Is.EqualTo(14_100_000));
    }

    [Test]
    public async Task StartIqTransfer_SendsCorrectCommand()
    {
        byte[] capturedCommand = null;
        _networkClient
            .Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Callback<byte[], CancellationToken>((cmd, _) => capturedCommand = cmd)
            .Returns(Task.CompletedTask);

        await _sut.StartIqTransferAsync();

        Assert.Multiple(() =>
        {
            Assert.That(capturedCommand[4], Is.EqualTo(0x80));
            Assert.That(capturedCommand[5], Is.EqualTo(0x02));
        });
    }

    [Test]
    public async Task StopIqTransfer_SendsCorrectCommand()
    {
        byte[] capturedCommand = null;
        _networkClient
            .Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Callback<byte[], CancellationToken>((cmd, _) => capturedCommand = cmd)
            .Returns(Task.CompletedTask);

        await _sut.StopIqTransferAsync();

        Assert.That(capturedCommand[5], Is.EqualTo(0x01));
    }

    [Test]
    public void Command_WhenNotConnected_ThrowsException()
    {
        _networkClient.Setup(x => x.IsConnected).Returns(false);
        Assert.ThrowsAsync<NetSdrClientException>(() => _sut.SetFrequencyAsync(14100000));
    }

    [Test]
    public void Command_WhenNakReceived_ThrowsException()
    {
        _networkClient
            .Setup(x => x.ReadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[] { 0x02, 0x00 });

        var ex = Assert.ThrowsAsync<NetSdrClientException>(() => _sut.SetFrequencyAsync(14100000));
        Assert.That(ex.Message, Does.Contain("Command failed"));
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}