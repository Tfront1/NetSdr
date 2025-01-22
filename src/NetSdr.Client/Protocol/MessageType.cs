namespace NetSdr.Client.Protocol;

[Flags]
public enum MessageType : byte
{
    // Host to Target
    SetControlItem = 0b000,
    RequestControlItem = 0b001,
    RequestControlItemRange = 0b010,
    DataItemAck = 0b011,
    HostDataItem0 = 0b100,
    HostDataItem1 = 0b101,
    HostDataItem2 = 0b110,
    HostDataItem3 = 0b111,

    // Target to Host
    ResponseToSetOrRequest = 0b000,
    UnsolicitedControlItem = 0b001,
    ResponseToRange = 0b010,
    DataItemAckFromTarget = 0b011,
    TargetDataItem0 = 0b100,
    TargetDataItem1 = 0b101,
    TargetDataItem2 = 0b110,
    TargetDataItem3 = 0b111
}