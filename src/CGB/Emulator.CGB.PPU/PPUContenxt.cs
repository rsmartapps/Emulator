using Emulator.CGB.Memory;
using Emulator.Domain.Tools;
using System;

namespace Emulator.CGB.PPU;

internal class PPUContenxt
{
    private ICGBMemoryBus _memory;
    #region LCD
    /*
     *  LCD & PPU enable: 0 = Off; 1 = On
     *  Window tile map area: 0 = 9800–9BFF; 1 = 9C00–9FFF
     *  Window enable: 0 = Off; 1 = On
     *  BG & Window tile data area: 0 = 8800–97FF; 1 = 8000–8FFF
     *  BG tile map area: 0 = 9800–9BFF; 1 = 9C00–9FFF
     *  OBJ size: 0 = 8×8; 1 = 8×16
     *  OBJ enable: 0 = Off; 1 = On
     *  BG & Window enable / priority [Different meaning in CGB Mode]: 0 = Off; 1 = On
     * */
    public const ushort LCD = 0xFF40;
    /// <summary>
    /// FF40
    /// </summary>
    public byte LcdData => _memory.Read(LCD);
    /// <summary>
    /// LCDC.0(FF40)
    /// </summary>
    public bool BGWindowPriority => BitOps.IsBit(LcdData, 0);
    /// <summary>
    /// LCDC.1(FF40)
    /// </summary>
    public bool OBJEnabled => BitOps.IsBit(LcdData, 1);
    /// <summary>
    /// LCDC.62(FF40)
    /// </summary>
    public int OBJSize
        =>(BitOps.IsBit(LcdData, 2)) switch 
        {
            false => 8,
            true => 16
        };
    /// <summary>
    /// LCDC.3(FF40)
    /// </summary>
    public ushort BGTileMap 
        => BitOps.IsBit(LcdData, 3) switch
        {
            false => 0x9800,
            true => 0x9C00
        };
    /// <summary>
    /// LCDC.4(FF40)
    /// </summary>
    public ushort BGWindowTile
        => BitOps.IsBit(LcdData, 4) switch
        {
            false => 0x8800,
            true => 0x8000
        };
    /// <summary>
    /// LCDC.5(FF40)
    /// </summary>
    public bool WindowEnabled => BitOps.IsBit(LcdData, 5);
    /// <summary>
    /// LCDC.6(FF40)
    /// </summary>
    public ushort WindowTileMap
        => BitOps.IsBit(LcdData, 6) switch
        {
            false => 0x9800,
            true => 0x9C00
        };
    /// <summary>
    /// LCDC.7(FF40)
    /// </summary>
    public bool EnabledLCDPPU => BitOps.IsBit(LcdData, 7);
    #endregion
    #region LCD Position and Scrolling
    /// <summary>
    /// Background viewport Y position address
    /// </summary>
    public const ushort SCYD = 0xFF42;
    /// <summary>
    /// Background viewport X position address
    /// </summary>
    public const ushort SCXD = 0xFF43;
    /// <summary>
    /// Window Y position address
    /// </summary>
    public const ushort WYD = 0xFF4A;
    /// <summary>
    /// Window X position address
    /// </summary>
    public const ushort WXD = 0xFF4B;
    public byte SCX
    {
        get => _memory.Read(SCXD);
        set => _memory.Write(SCXD, value);
    }
    #endregion
    #region STAT

    public const ushort STATUS = 0xFF41;
    public const ushort LCD_Y = 0xFF44;
    public const ushort LYC_Y = 0xFF45;

    public byte StatusData
    {
        get => _memory.Read(STATUS);
        set => _memory.Write(STATUS, value);
    }
    public byte LCDY => _memory.Read(LCD_Y);
    public byte LYCY { 
        get => _memory.Read(LYC_Y); 
        set => _memory.Write(LYC_Y, value); 
    }

    public LCDMode Mode
    {
        get => (BitOps.IsBit(StatusData, 1), BitOps.IsBit(StatusData, 0)) switch
        {
            (false, false) => LCDMode.HBlank,
            (false, true) => LCDMode.VBlank,
            (true, false) => LCDMode.OAM,
            (true, true) => LCDMode.Render
        };
        set { 
            switch (value)
            {
                case LCDMode.HBlank:
                    BitOps.bitClear(StatusData, 0);
                    BitOps.bitClear(StatusData, 1);
                    break;
                case LCDMode.VBlank:
                    BitOps.bitSet(StatusData, 0);
                    BitOps.bitClear(StatusData, 1);
                    break;
                case LCDMode.OAM:
                    BitOps.bitClear(StatusData, 0);
                    BitOps.bitSet(StatusData, 1);
                    break;
                case LCDMode.Render:
                    BitOps.bitSet(StatusData, 1);
                    BitOps.bitSet(StatusData, 1);
                    break;

            }
        }
    }
        
    #endregion
    #region Palette
    /// <summary>
    /// Background color palette specification
    /// Bacground palette index
    /// </summary>
    private const ushort BCPS_BGPI = 0xFF68;
    public byte BGPaletteSpecification => _memory.Read(BCPS_BGPI);
    /// <summary>
    /// Background color pallete data
    /// Background palette data
    /// </summary>
    private const ushort BCPD_BGPD = 0xFF69;
    public byte BGPaletteData => _memory.Read(BCPD_BGPD);

    #endregion

    public PPUContenxt(ICGBMemoryBus memory)
    {
        this._memory = memory;
    }

    //public int[,] GetRowTilePixels(ushort address)
    //{

    //}
    public void PrepareRow(ushort address)
    {
        /*
         * The Game Boy contains two 32×32 tile maps in VRAM at the memory 
         * areas $9800-$9BFF and $9C00-$9FFF. Any of these maps can be used 
         * to display the Background or the Window.
         */

        /*
        Block	VRAM Address        Corresponding      Tile    IDs
                                Objects	    BG/Win if LCDC.4=1	    BG/Win if LCDC.4=0
        0	    $8000–$87FF	    0–127	    0–127	
        1	    $8800–$8FFF	    128–255	    128–255	                128–255 (or -128–-1)
        2	    $9000–$97FF	        (Can't use)	                    0–127
        */

        for (; address < address + 32; address++)
        {
            var objPointer = _memory.Read(address);
            ushort baseAddres = (objPointer, OBJEnabled, WindowEnabled, BGWindowPriority) switch
            {
                ( <= 127, true, false, false) => 0x8000,
                ( <=255, true, false, false) => 0x8800,
                ( <=255, false, false, false) => 0x8800,
                _ => throw new Exception()
            };
        }


    }
}
