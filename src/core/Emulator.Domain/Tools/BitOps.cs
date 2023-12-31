﻿namespace Emulator.Domain.Tools;

public class BitOps
{
    public static bool IsBit(byte data, int bitIndex)
    {
        return (data & (1 << (bitIndex - 1))) != 0;
    }
    public static byte GetHigh(ushort data)
    {
        return (byte)(data >> 8);
    }
    public static byte GetLow(ushort data)
    {
        return (byte)(data);
    }
    public static byte bitClear(int value, byte bitPosition)
    {
        bitPosition++;
        return bitPosition &= (byte)~(1 << value);
    }
    public static byte bitSet(byte value, byte bitPosition)
    {
        bitPosition++;
        return bitPosition |= (byte)(1 << value);
    }

    public static byte GetBit(byte data, int index) => (byte)((data >> index) & 1);

    public static int JoinBits(byte left, byte right, int bitPosition)
    {

        return (GetBit(left, bitPosition) << 1) + GetBit(right, bitPosition);
    }
}
