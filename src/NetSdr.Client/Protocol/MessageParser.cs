namespace NetSdr.Client.Protocol;

internal static class MessageParser
{
    public static bool TryParseMessage(
        byte[] data,
        out MessageHeader header,
        out ControlItem? controlItem,
        out byte[] parameters)
    {
        header = default;
        controlItem = null;
        parameters = Array.Empty<byte>();

        try
        {
            if (data.Length < 4)
                return false;

            var headerBytes = new byte[2];
            Array.Copy(data, 0, headerBytes, 0, 2);
            header = MessageHeader.FromBytes(headerBytes);

            if (data.Length < header.Length)
                return false;

            controlItem = (ControlItem)BitConverter.ToUInt16(data, 2);

            var paramLength = header.Length - 4;
            if (paramLength > 0)
            {
                parameters = new byte[paramLength];
                Array.Copy(data, 4, parameters, 0, paramLength);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
