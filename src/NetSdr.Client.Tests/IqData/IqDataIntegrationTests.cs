namespace NetSdr.Client.Tests.IqData;

[TestFixture]
[Explicit]
public class IqDataIntegrationTests
{
    private string _tempFilePath;
    private const int TcpPort = 50000;

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

    [Test]
    public async Task FullWorkflow_CapturesAndSavesData()
    {
        // This test requires a running NetSDR device
        // Should be marked with [Explicit] in real test suite

        // Arrange
        using var client = new NetSdrClient(new NetworkClient());

        // Act
        await client.ConnectAsync("localhost", TcpPort);
        await client.SetFrequencyAsync(14100000);
        await client.StartIqTransferAsync();

        // Wait for some data
        await Task.Delay(1000);

        await client.StopIqTransferAsync();
        await client.DisconnectAsync();

        // Assert
        var fileInfo = new FileInfo(_tempFilePath);
        Assert.That(fileInfo.Length, Is.GreaterThan(0));
    }
}
