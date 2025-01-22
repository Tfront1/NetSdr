namespace NetSdr.Client.Protocol;

public class ControlMessage
{
    public MessageHeader Header { get; }
    public ControlItem ControlItem { get; }
    public ReadOnlyMemory<byte> Parameters { get; }

    public ControlMessage(MessageType type, ControlItem controlItem, ReadOnlyMemory<byte> parameters)
    {
        var length = 4 + parameters.Length; // Header (2) + ControlItem (2) + Parameters
        Header = new MessageHeader(length, type);
        ControlItem = controlItem;
        Parameters = parameters;
    }

    public byte[] ToBytes()
    {
        var result = new byte[Header.Length];
        var headerBytes = Header.ToBytes();
        var controlItemBytes = BitConverter.GetBytes((ushort)ControlItem);

        Buffer.BlockCopy(headerBytes, 0, result, 0, 2);
        Buffer.BlockCopy(controlItemBytes, 0, result, 2, 2);

        if (!Parameters.IsEmpty)
        {
            Parameters.Span.CopyTo(new Span<byte>(result, 4, Parameters.Length));
        }

        return result;
    }

    public static ControlMessage FromBytes(ReadOnlyMemory<byte> data)
    {
        if (data.Length < 4)
            throw new ArgumentException("Data too short", nameof(data));

        var header = MessageHeader.FromBytes(data.Span.Slice(0, 2));
        var controlItem = (ControlItem)BitConverter.ToUInt16(data.Span.Slice(2, 2));
        var parameters = data.Length > 4 ? data.Slice(4) : ReadOnlyMemory<byte>.Empty;

        return new ControlMessage(header.Type, controlItem, parameters);
    }
}
