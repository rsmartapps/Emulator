namespace Emulator.CGB.Memory;

using MBC;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.Arm;

/// <=summary>
/// Start	End	Description	Notes
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
/// <=/summary>
public class CGBMemoryBus : ICGBMemoryBus
{
    const ushort ROM = 0x3FFF;
    const ushort ROM_BANK = 0x7FFF;
    const ushort VRAM = 0x9FFF;
    const ushort SRAM = 0xBFFF;
    const ushort WRAM = 0xCFFF;
    const ushort WRAME = 0xDFFF;
    const ushort MIRRORED_RAM = 0xFDFF;
    const ushort OAM = 0xFE9F;
    const ushort UM = 0xFEFF;
    const ushort IO = 0xFF7F;
    const ushort HRAM = 0xFFFE;
    const ushort INTERRUPT = 0xFFFF;
    public IMBC Cartridge { get; set; }

    /// <=summary>
    /// Addresses: 0000h - FFFFh
    /// The lower 32KB of the address space is reserved for ROM, where the game's program code is stored. The Game Boy cartridge contains the game's ROM.
    /// The DMG uses bank switching to access additional ROM banks if present in the cartridge.
    /// <=/summary>
    IDictionary<ushort, byte> RAM = new Dictionary<ushort, byte>(0xFFFF);
    /// <=summary>
    /// Addresses: 8000h - 9FFFh
    /// This memory region stores the graphics data used for rendering sprites, backgrounds, and tiles on the screen. It is organized into tile maps and tile data.
    /// <=/summary>
    IDictionary<ushort, byte> V_RAM = new Dictionary<ushort, byte>(0x9FFF - 0x8000);

    public void InsertCartridge(byte[] file)
    {
        Cartridge = MBCManager.LoadROM(file);
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
                case <= ROM:
                    return Cartridge.ReadLowRom(address);
                case <= ROM_BANK:
                    return Cartridge.ReadBankRom(address, ROM_BANK);
                case <= VRAM:
                    return V_RAM[(ushort)(address - 0x8000)];
                case <= SRAM:
                    return Cartridge.ReadSRam(address);
                case <= WRAM:
                    switch (WRamBank)
                    {
                        case < 2: return RAM[address];
                        default: return Cartridge.ReadBankRam(address, WRamBank);
                    }
                case <= WRAME:
                    return Read(RAM[0xDFFF & 0x1FFF]);
                case <= MIRRORED_RAM:
                    return 0;
                case <= OAM:
                    // TODO: 
                    /*FEA0 - FEFF range
                    Nintendo indicates use of this area is prohibited.This area returns $FF when OAM is blocked, and otherwise the behavior depends on the hardware revision.
                    On DMG, MGB, SGB, and SGB2, reads during OAM block trigger OAM corruption.Reads otherwise return $00.
                    On CGB revisions 0 - D, this area is a unique RAM area, but is masked with a revision-specific value.
                    On CGB revision E, AGB, AGS, and GBP, it returns the high nibble of the lower address byte twice, e.g.FFAx returns $AA, FFBx returns $BB, and so forth.
                    */
                    return RAM[address];
                case <= UM:
                    return 0;
                case <= IO:
                    return RAM[address];
                case <= HRAM:
                    return RAM[address];
                case INTERRUPT:
                    return RAM[address];
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
                case <= ROM: break;
                case <= ROM_BANK: break;
                case <= VRAM: V_RAM[(ushort)(address - 0x8000)] = value; break;
                case <= SRAM: Cartridge.WriteSRam(address, value); break;
                case <= WRAM: RAM[address] = value; break;
                case <= WRAME: RAM[0xDFFF & 0x1FFF] = value; break;
                case <= MIRRORED_RAM: break;
                case <= OAM:
                    // TODO: 
                    /*FEA0 - FEFF range
                    Nintendo indicates use of this area is prohibited.This area returns $FF when OAM is blocked, and otherwise the behavior depends on the hardware revision.
                    On DMG, MGB, SGB, and SGB2, reads during OAM block trigger OAM corruption.Reads otherwise return $00.
                    On CGB revisions 0 - D, this area is a unique RAM area, but is masked with a revision-specific value.
                    On CGB revision E, AGB, AGS, and GBP, it returns the high nibble of the lower address byte twice, e.g.FFAx returns $AA, FFBx returns $BB, and so forth.
                    */
                    RAM[address] = value; break;
                case <= UM: break;
                case <= IO: RAM[address] = value; break;
                case <= HRAM: RAM[address] = value; break;
                case INTERRUPT: RAM[address] = value; break;
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Error reading from {address.ToString("X")}");
        }
    }
    
    #region I/O Ranges
    /*
    Start   End     First       Purpose
                    appeared      
    $FF00           DMG         Joypad input
    $FF01	$FF02   DMG         Serial transfer
    $FF04	$FF07   DMG         Timer and divider
    $FF10	$FF26   DMG         Audio
    $FF30	$FF3F   DMG         Wave pattern
    $FF40	$FF4B   DMG         LCD Control, Status, Position, Scrolling, and Palettes
    $FF4F           CGB         VRAM Bank Select
    $FF50           DMG         Set to non-zero to disable boot ROM
    $FF51	$FF55   CGB         VRAM DMA
    $FF68	$FF6B   CGB         BG / OBJ Palettes
    $FF70           CGB         WRAM Bank Select
    */
    public byte JoyPad { get { return Read(0xFF00); } set { Write(0xFF00, value); } }
    public byte VRamBank { get { return Read(0xFF4F); } set { Write(0xFF4F, value); } }
    /// <summary>
    /// Set to non-zero to disable boot ROM
    /// </summary>
    public byte DisableROM { get { return Read(0xFF50); } set { Write(0xFF50, value); } }
    public byte WRamBank { get { return Read(0xFF70); } set { Write(0xFF70, value); } }
    #endregion

    #region Interrupts
    /// <=summary>
    /// https://gbdev.io/pandocs/Interrupts.html#interrupts
    /// <=/summary>
    //Interrupt IO Flags
    //Bit 0: V-Blank Interrupt Enable(INT 40h)  (1=Enable)
    //Bit 1: LCD STAT Interrupt Enable(INT 48h)  (1=Enable)
    //Bit 2: Timer Interrupt Enable(INT 50h)  (1=Enable)
    //Bit 3: Serial Interrupt Enable(INT 58h)  (1=Enable)
    //Bit 4: Joypad Interrupt Enable(INT 60h)  (1=Enable)
    public byte IE { get { return Read(0xFFFF); } set { Write(0xFFFF, value); } }//FFFF - IE - Interrupt Enable (R/W)
    public byte IF { get { return Read(0xFF0F); } set { Write(0xFF0F, value); } }//FF0F - IF - Interrupt Flag (R/W)
    #endregion
}