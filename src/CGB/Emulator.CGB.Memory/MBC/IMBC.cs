namespace Emulator.CGB.Memory.MBC;

public interface IMBC
{
    byte ReadLowRom(ushort address);
    byte ReadBankRom(ushort address, ushort romBank);
    byte ReadSRam(ushort address);
    void WriteSRam(ushort address, byte value);
    byte ReadBankRam(ushort address, byte wRamBank);
}
