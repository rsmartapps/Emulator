namespace Emulator.CGB.Memory.MBC;

internal static class MBCManager
{
    public static IMBC LoadROM(byte[] ROM)
    {
        IMBC mbc = null;
        byte cartirdigeType = 0;
        try { cartirdigeType = ROM[0x147]; } catch { }
        switch (cartirdigeType)
        {
            case 0: mbc = new MBC0(ROM); break;
            case <= 0x03: mbc = new MBC1(ROM); break;
            case <= 0x06: break;
            case <= 0x13: break;
            case <= 0x1B: break;
            default:
                Console.WriteLine($"CartRidge Type coudn't be matched {cartirdigeType.ToString("X")}-{cartirdigeType.ToString("x2")}");
                break;
        }

        return mbc;
    }

    /*
     * Types
     *  MBC1 2MByte and/or 32KByte RAM
     *  MBC2 256KByte ROM and 512x4 bits RAM
     *  MBC3 2MByte and 64KByte RAM and Timer
     *  MBC5 8MByte and 128KByte RAM
     */
}
