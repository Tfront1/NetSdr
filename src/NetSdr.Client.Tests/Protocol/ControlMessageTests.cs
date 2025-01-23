using NetSdr.Client.Protocol;

namespace NetSdr.Client.Tests.Protocol;

[TestFixture]
public class ControlMessageTests
{
    [Test]
    public void Constructor_WithValidParams_CreatesMessage()
    {
        // Arrange
        var parameters = new byte[] { 1, 2, 3 };

        // Act
        var message = new ControlMessage(
            MessageType.SetControlItem,
            ControlItem.ReceiverState,
            parameters);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(message.Header.Type, Is.EqualTo(MessageType.SetControlItem));
            Assert.That(message.ControlItem, Is.EqualTo(ControlItem.ReceiverState));
            Assert.That(message.Parameters.ToArray(), Is.EqualTo(parameters));
        });
    }

    [Test]
    public void ToBytes_ReturnsCorrectByteArray()
    {
        // Arrange
        var parameters = new byte[] { 1, 2, 3 };
        var message = new ControlMessage(
            MessageType.SetControlItem,
            ControlItem.ReceiverState,
            parameters);

        // Act
        var bytes = message.ToBytes();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(bytes.Length, Is.EqualTo(7)); // 2 (header) + 2 (control item) + 3 (parameters)
            Assert.That(bytes[4], Is.EqualTo(1)); // First parameter byte
            Assert.That(bytes[5], Is.EqualTo(2)); // Second parameter byte
            Assert.That(bytes[6], Is.EqualTo(3)); // Third parameter byte
        });
    }
}
