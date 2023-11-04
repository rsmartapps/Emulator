using Emulator.Domain;
using Emulator.GBC.MBC;
using System.Runtime.Intrinsics.Arm;

namespace Emulator.GBC;
/// <summary>
/// https://gbdev.io/pandocs/Memory_Map.html#io-ranges
/// The Game Boy has a 16-bit address bus, which is used to address ROM, RAM, and I/O.
/// Start End Description Notes
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
/// </summary>
public class GBCMemory : MachineMemory
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
    const ushort INTERRUPT = 0xFFFF;
    IMBC MBC;
    /// <summary>
    /// Addresses: 0000h - 7FFFh
    /// The lower 32KB of the address space is reserved for ROM, where the game's program code is stored. The Game Boy cartridge contains the game's ROM.
    /// The DMG uses bank switching to access additional ROM banks if present in the cartridge.
    /// </summary>
    byte[] RAM  = new byte[0xFFFF];
    /// <summary>
    /// Addresses: 8000h - 9FFFh
    /// This memory region stores the graphics data used for rendering sprites, backgrounds, and tiles on the screen. It is organized into tile maps and tile data.
    /// </summary>
    byte[] VRAM = new byte[0x9FFF];
    /// <summary>
    /// Addresses: A000h - BFFFh
    /// Cartridges may contain additional RAM for save game data or other purposes.This region is used to access that external RAM.
    /// </summary>

    public override void Load(byte[] file)
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
    public override byte Read(ushort address)
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
                    return VRAM[address];
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
                case INTERRUPT:
                    return RAM[address];
                default:
                    Console.WriteLine($"Out of index {address.ToString("X")}");
                    return 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading from {address.ToString("X")} with error {ex.Message}");
        }
        return value;
    }

    public override void Write(ushort address, byte value)
    {
        try
        {
            switch (address)
            {
                case < ROM__BANK_HIGH:
                     RAM[address] = value; break;
                case < VRAM_HIGH:
                     VRAM[address - 0x8000] = value; break;
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
            Console.WriteLine($"Error reading from {address.ToString("X")}");
        }
    }
}
