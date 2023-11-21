namespace Emulator.CGB.Memory.MBC;

public class MBC0 : IMBC
{
    protected byte[] ROM;

    public IDictionary<ushort, byte> RAM { get; } = new Dictionary<ushort, byte>();

    public MBC0(byte[] rom)
    {
        this.ROM = rom;
    }

    public byte ReadLowRom(ushort address)
    {
        return ROM[address];
    }


    public byte ReadSRam(ushort address)
    {
        throw new NotImplementedException();
    }

    public void WriteSRam(ushort address, byte value)
    {
        throw new NotImplementedException();
    }

    public byte ReadBankRom(ushort address, ushort romBank)
    {
        throw new NotImplementedException();
    }

    public byte ReadBankRam(ushort address, byte wRamBank)
    {
        throw new NotImplementedException();
    }
}
