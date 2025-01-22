namespace NetSdr.Client.Protocol;

internal static class NetSdrProtocol
{
    public static bool IsNakMessage(ReadOnlySpan<byte> message)
    {
        return message.Length == 2 && message[0] == 0x02 && message[1] == 0x00;
    }

    public static byte[] CreateAckMessage(byte dataItem)
    {
        var header = new MessageHeader(3, MessageType.DataItemAck);
        var message = new byte[3];
        var headerBytes = header.ToBytes();

        Buffer.BlockCopy(headerBytes, 0, message, 0, 2);
        message[2] = dataItem;

        return message;
    }

    public static bool IsStartOfTransmission(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4) return false;

        // Check for sequence number 0 in IQ data packet
        return data[2] == 0x00 && data[3] == 0x00;
    }

    public static uint GetSequenceNumber(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
            throw new ArgumentException("Data too short for sequence number", nameof(data));

        return BitConverter.ToUInt16(data.Slice(2, 2));
    }
}
