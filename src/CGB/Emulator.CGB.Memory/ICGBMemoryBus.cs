namespace Emulator.CGB.Memory
{
    internal interface ICGBMemoryBus
    {
        byte Read(ushort address);
        void Write(ushort address, byte value);
    }
}