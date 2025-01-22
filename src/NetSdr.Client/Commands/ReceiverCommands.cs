using NetSdr.Client.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSdr.Client.Commands;

internal static class ReceiverCommands
{
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