using NetSdr.Client.Protocol;

namespace NetSdr.Client.Commands;

/// <summary>
/// Contains factory methods for creating receiver control commands according to NetSDR protocol.
/// </summary>
public static class ReceiverCommands
{
    /// <summary>
    /// Creates command to start IQ data transfer.
    /// </summary>
    /// <param name="use24Bit">If true, use 24-bit data mode. Default is 16-bit.</param>
    /// <returns>Command bytes for starting IQ transfer</returns>
    public static byte[] CreateStartIqTransferCommand(bool use24Bit = false)
    {
        var parameters = new byte[4];
        parameters[0] = 0x80;  // Complex I/Q mode
        parameters[1] = 0x02;  // Run state
        parameters[2] = (byte)(use24Bit ? 0x80 : 0x00);  // Bit depth
        parameters[3] = 0x00;  // FIFO size (unused in continuous mode)
        return new ControlMessage(
            MessageType.SetControlItem,
            ControlItem.ReceiverState,
            parameters).ToBytes();
    }

    /// <summary>
    /// Creates command to stop IQ data transfer.
    /// </summary>
    /// <returns>Command bytes for stopping IQ transfer</returns>
    public static byte[] CreateStopIqTransferCommand()
    {
        var parameters = new byte[4];
        parameters[0] = 0x00;  // Parameter values don't matter for stop
        parameters[1] = 0x01;  // Stop state
        parameters[2] = 0x00;
        parameters[3] = 0x00;
        return new ControlMessage(
            MessageType.SetControlItem,
            ControlItem.ReceiverState,
            parameters).ToBytes();
    }

    /// <summary>
    /// Creates command to set receiver frequency.
    /// </summary>
    /// <param name="frequency">Target frequency in Hz</param>
    /// <returns>Command bytes for frequency change</returns>
    public static byte[] CreateSetFrequencyCommand(uint frequency)
    {
        var parameters = new byte[6];
        parameters[0] = 0x00;  // Channel 1

        // Convert frequency to bytes (40-bit value, but we use 32-bit)
        var freqBytes = BitConverter.GetBytes(frequency);
        Buffer.BlockCopy(freqBytes, 0, parameters, 1, 4);
        parameters[5] = 0x00;  // MSB of 40-bit value

        return new ControlMessage(
            MessageType.SetControlItem,
            ControlItem.ReceiverFrequency,
            parameters).ToBytes();
    }
}