namespace NetSdr.Client.Protocol;

public readonly struct MessageHeader
{
    private const int LengthMask = 0x1FFF;  // 13 bits for length
    private const int TypeShift = 13;        // 3 bits for type, shifted

    public int Length { get; }
    public MessageType Type { get; }

    public MessageHeader(int length, MessageType type)
    {
        if (length > LengthMask)
            throw new ArgumentOutOfRangeException(nameof(length),
                "Message length exceeds maximum allowed value (8191 bytes)");

        Length = length;
        Type = type;
    }

    public byte[] ToBytes()
    {
        var value = (Length & LengthMask) | ((int)Type << TypeShift);
        return new[] { (byte)(value & 0xFF), (byte)(value >> 8) };
    }

    public static MessageHeader FromBytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 2)
            throw new ArgumentException("Invalid header bytes", nameof(bytes));

        var value = bytes[0] | (bytes[1] << 8);
        var length = value & LengthMask;
        var type = (MessageType)((value >> TypeShift) & 0x07);

        return new MessageHeader(length, type);
    }
}