namespace Emulator.CGB.Memory.MBC;

internal interface IMBC
{
    public IDictionary<ushort, byte> RAM { get; }
    byte Read(ushort address);
    void Write(ushort address, byte value);
}
