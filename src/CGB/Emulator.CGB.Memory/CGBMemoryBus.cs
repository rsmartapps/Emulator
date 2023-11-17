namespace Emulator.CGB.Memory;

using MBC;

internal class CGBMemoryBus : ICGBMemoryBus
{
    const ushort ROM_HIGH = 0x3FFF;
    const ushort ROM__BANK_HIGH = 0x7FFF;
    const ushort VRAM_HIGH = 0x9FFF;
    const ushort CRAM_HIGH = 0xBFFF;
    const ushort WRAM_HIGH = 0xDFFF;
    const ushort WRAME_HIGH = 0xFDFF;
    const ushort OAM_HIGH = 0xFE9F;
    const ushort UM_HIGH = 0xFEFF;
    const ushort IO_HIGH = 0xFF7F;
    const ushort ZPRAM_HIGH = 0xFFFE;
    /// <summary>
    /// https://gbdev.io/pandocs/Interrupts.html#interrupts
    /// </summary>
    //Interrupt IO Flags
    //Bit 0: V-Blank Interrupt Enable(INT 40h)  (1=Enable)
    //Bit 1: LCD STAT Interrupt Enable(INT 48h)  (1=Enable)
    //Bit 2: Timer Interrupt Enable(INT 50h)  (1=Enable)
    //Bit 3: Serial Interrupt Enable(INT 58h)  (1=Enable)
    //Bit 4: Joypad Interrupt Enable(INT 60h)  (1=Enable)
    public byte IE { get { return Read(0xFFFF); } set { Write(0xFFFF, value); } }//FFFF - IE - Interrupt Enable (R/W)
    public byte IF { get { return Read(0xFF0F); } set { Write(0xFF0F, value); } }//FF0F - IF - Interrupt Flag (R/W)
    IMBC MBC;
    /// <summary>
    /// Addresses: 0000h - FFFFh
    /// The lower 32KB of the address space is reserved for ROM, where the game's program code is stored. The Game Boy cartridge contains the game's ROM.
    /// The DMG uses bank switching to access additional ROM banks if present in the cartridge.
    /// </summary>
    IDictionary<ushort, byte> RAM = new Dictionary<ushort, byte>(0xFFFF);
    /// <summary>
    /// Addresses: 8000h - 9FFFh
    /// This memory region stores the graphics data used for rendering sprites, backgrounds, and tiles on the screen. It is organized into tile maps and tile data.
    /// </summary>
    IDictionary<ushort, byte> VRAM = new Dictionary<ushort, byte>(0x9FFF);

    public void Load(byte[] file)
    {
        MBC = MBCManager.LoadROM(file);
    }

    /// 0000	3FFF	16 KiB ROM bank 00	From cartridge, usually a fixed bank
    /// 4000    7FFF    16 KiB ROM Bank 01~NN   From cartridge, switchable bank via mapper(if any)
    /// 8000    9FFF    8 KiB Video RAM(VRAM)  In CGB mode, switchable bank 0 / 1
    /// A000    BFFF    8 KiB External RAM  From cartridge, switchable bank if any
    /// C000    CFFF    4 KiB Work RAM(WRAM)
    /// D000    DFFF    4 KiB Work RAM(WRAM)   In CGB mode, switchable bank 1~7
    /// E000    FDFF    Mirror of C000~DDFF(ECHO RAM)  Nintendo says use of this area is prohibited.
    /// FE00    FE9F    Object attribute memory(OAM)
    /// FEA0    FEFF    Not Usable  Nintendo says use of this area is prohibited
    /// FF00    FF7F    I / O Registers
    /// FF80    FFFE    High RAM(HRAM)
    /// FFFF    FFFF    Interrupt Enable register(IE)
    public byte Read(ushort address)
    {
        byte value = 0;
        try
        {
            switch (address)
            {
                case < ROM_HIGH:
                    return MBC.Read(address);
                case < ROM__BANK_HIGH:
                    return MBC.Read(address);
                case < VRAM_HIGH:
                    return VRAM[(ushort)(address - 0x8000)];
                case < CRAM_HIGH:
                    return RAM[address];
                case < WRAM_HIGH:
                    return RAM[address];
                case < WRAME_HIGH:
                    return RAM[address];
                case < OAM_HIGH:
                    return RAM[address];
                case < UM_HIGH:
                    return 0xFF;
                case < IO_HIGH:
                    return RAM[address];
                case < ZPRAM_HIGH:
                    return RAM[address];
                default:
                    Console.WriteLine($"Out of index {address.ToString("X")}");
                    return 0;
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Error reading from {address.ToString("X")} with error {ex.Message}");
        }
        return value;
    }

    public void Write(ushort address, byte value)
    {
        try
        {
            switch (address)
            {
                case < ROM_HIGH:
                    MBC.Write(address, value); break;
                case < ROM__BANK_HIGH:
                    MBC.Write(address, value); break;
                case < VRAM_HIGH:
                    VRAM[(ushort)(address - 0x8000)] = value; break;
                case < CRAM_HIGH:
                    RAM[address] = value; break;
                case < WRAM_HIGH:
                    RAM[address] = value; break;
                case < OAM_HIGH:
                    RAM[address] = value; break;
                case < IO_HIGH:
                    RAM[address] = value; break;
                case < ZPRAM_HIGH:
                    RAM[address] = value; break;
                default:
                    Console.WriteLine($"Can't write heare {address.ToString("X")}");
                    break;
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Error reading from {address.ToString("X")}");
        }
    }
}