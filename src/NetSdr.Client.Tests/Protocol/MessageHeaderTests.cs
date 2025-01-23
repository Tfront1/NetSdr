using NetSdr.Client.Protocol;

namespace NetSdr.Client.Tests.Protocol;

[TestFixture]
public class MessageHeaderTests
{
    [Test]
    public void Constructor_WithValidParams_CreatesHeader()
    {
        // Arrange & Act
        var header = new MessageHeader(10, MessageType.SetControlItem);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(header.Length, Is.EqualTo(10));
            Assert.That(header.Type, Is.EqualTo(MessageType.SetControlItem));
        });
    }

    [Test]
    public void Constructor_WithTooLargeLength_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MessageHeader(0x2000, MessageType.SetControlItem));
    }

    [Test]
    public void ToBytes_ReturnsCorrectByteArray()
    {
        // Arrange
        var header = new MessageHeader(10, MessageType.SetControlItem);

        // Act
        var bytes = header.ToBytes();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(bytes.Length, Is.EqualTo(2));
            Assert.That(bytes[0], Is.EqualTo(10));  // LSB
            Assert.That(bytes[1], Is.EqualTo(0));   // MSB with type bits
        });
    }

    [Test]
    public void FromBytes_WithValidData_ReturnsCorrectHeader()
    {
        // Arrange
        var bytes = new byte[] { 10, 0 };

        // Act
        var header = MessageHeader.FromBytes(bytes);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(header.Length, Is.EqualTo(10));
            Assert.That(header.Type, Is.EqualTo(MessageType.SetControlItem));
        });
    }

    [Test]
    public void FromBytes_WithInvalidData_ThrowsArgumentException()
    {
        // Arrange
        var bytes = new byte[] { 10 }; // Too short

        // Act & Assert
        Assert.Throws<ArgumentException>(() => MessageHeader.FromBytes(bytes));
    }
}
