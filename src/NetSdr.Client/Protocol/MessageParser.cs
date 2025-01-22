namespace NetSdr.Client.Protocol;

internal static class MessageParser
{
    public static bool TryParseMessage(ReadOnlySpan<byte> data,
        out MessageHeader header,
        out ControlItem? controlItem,
        out ReadOnlySpan<byte> parameters)
    {
        header = default;
        controlItem = null;
        parameters = default;

        try
        {
            if (data.Length < 4)
                return false;

            header = MessageHeader.FromBytes(data.Slice(0, 2));

            if (data.Length < header.Length)
                return false;

            controlItem = (ControlItem)BitConverter.ToUInt16(data.Slice(2, 2));
            parameters = data.Slice(4, header.Length - 4);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
