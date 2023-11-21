using Emulator.CGB.Memory.MBC;
using Emulator.Domain;

namespace Emulator.CGB.Memory
{
    public interface ICGBMemoryBus :IMemoryBus
    {
        public IMBC Cartridge { get; set; }
        public byte Read(ushort address);
        public void Write(ushort address, byte value);
    }
}