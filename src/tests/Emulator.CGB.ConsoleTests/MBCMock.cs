using Emulator.CGB.Memory;
using Emulator.CGB.Memory.MBC;

namespace Emulator.CGB.ConsoleTests;

public class MBCMock : ICGBMemoryBus
{
    public IDictionary<ushort, byte> RAM { get; } = new Dictionary<ushort, byte>();
    public IMBC Cartridge { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public byte Read(ushort address)
    {
        RAM.TryGetValue(address, out byte value);
        return value;
    }

    public void Write(ushort address, byte value)
    {
        if(!RAM.TryAdd(address, value))
            RAM[address] = value;
    }
}
