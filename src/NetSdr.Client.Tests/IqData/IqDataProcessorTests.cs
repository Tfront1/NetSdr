using Microsoft.Extensions.Logging;
using Moq;
using NetSdr.Client.IqData;
using System.Net.Sockets;
using System.Net;

namespace NetSdr.Client.Tests.IqData;

[TestFixture]
public class IqDataProcessorTests
{
    private string _tempFilePath;
    private Mock<ILogger<IqDataProcessor>> _loggerMock;
    private IqDataProcessor _processor;
    private const int UdpPort = 60000;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _tempFilePath = Path.GetTempFileName();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<IqDataProcessor>>();
        _processor = new IqDataProcessor(_tempFilePath, UdpPort, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _processor.Dispose();
    }

    [Test]
    public async Task ProcessData_WritesCorrectDataToFile()
    {
        // Arrange
        using var udpClient = new UdpClient();
        var testData = GenerateTestIqData();
        _processor.Start();

        // Act
        await udpClient.SendAsync(testData, testData.Length,
            new IPEndPoint(IPAddress.Loopback, UdpPort));
        await Task.Delay(100); // Give some time for processing
        await _processor.StopAsync();

        // Assert
        using var fileStream = new FileStream(_tempFilePath, FileMode.Open);
        var fileData = new byte[testData.Length - 4]; // Minus header size
        await fileStream.ReadAsync(fileData);

        Assert.Multiple(() =>
        {
            Assert.That(fileData.Length, Is.EqualTo(testData.Length - 4));
            Assert.That(fileData, Is.EqualTo(testData.Skip(4).ToArray()));
        });
    }

    [Test]
    public void Start_WhenAlreadyStarted_ThrowsException()
    {
        // Arrange
        _processor.Start();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _processor.Start());
    }

    [Test]
    public async Task StopAsync_WhenNotStarted_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _processor.StopAsync());
    }

    [Test]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            _processor.Dispose();
            _processor.Dispose();
        });
    }

    [Test]
    public async Task ProcessData_WithInvalidPacket_LogsWarning()
    {
        // Arrange
        using var udpClient = new UdpClient();
        var invalidData = new byte[2]; // Too small packet
        _processor.Start();

        // Act
        await udpClient.SendAsync(invalidData, invalidData.Length,
            new IPEndPoint(IPAddress.Loopback, UdpPort));
        await Task.Delay(100);
        await _processor.StopAsync();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce());
    }

    [Test]
    public async Task ProcessData_HandlesMultiplePackets()
    {
        // Arrange
        using var udpClient = new UdpClient();
        var packet1 = GenerateTestIqData(1);
        var packet2 = GenerateTestIqData(2);
        _processor.Start();

        // Act
        await udpClient.SendAsync(packet1, packet1.Length,
            new IPEndPoint(IPAddress.Loopback, UdpPort));
        await udpClient.SendAsync(packet2, packet2.Length,
            new IPEndPoint(IPAddress.Loopback, UdpPort));
        await Task.Delay(100);
        await _processor.StopAsync();

        // Assert
        var fileInfo = new FileInfo(_tempFilePath);
        Assert.That(fileInfo.Length, Is.EqualTo((packet1.Length - 4) + (packet2.Length - 4)));
    }

    private static byte[] GenerateTestIqData(byte sequenceNumber = 1)
    {
        var data = new byte[1028]; // 4 header + 1024 data bytes

        // Header
        data[0] = 0x04; // Header LSB
        data[1] = 0x84; // Header MSB
        data[2] = sequenceNumber; // Sequence number LSB
        data[3] = 0x00; // Sequence number MSB

        // Generate some test I/Q data
        for (int i = 4; i < data.Length; i++)
        {
            data[i] = (byte)(i & 0xFF);
        }

        return data;
    }
}