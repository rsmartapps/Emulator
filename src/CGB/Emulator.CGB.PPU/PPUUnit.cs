using Emulator.CGB.Memory;
using Emulator.CGB.Memory.MBC;
using Emulator.Domain;
using Emulator.Domain.Tools;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;

namespace Emulator.CGB.PPU;

public class PPUUnit : IPPU
{
    private const int SCREEN_WIDTH = 160;
    private const int SCREEN_HEIGHT = 144;
    private const int SCREEN_VBLANK_HEIGHT = 153;

    private const int OAM_DOTS = 80;
    private const int RENDER_DOTS = 172;
    private const int SCANLINE_DOTS = 456;

    private const int VBLANK_INTERRUPT = 0;
    private const int LCD_INTERRUPT = 1;


    private ICGBMemoryBus _Memory { get; }
    private PPUContenxt _context;
    private int _TCycles;
    private long[,] Screen = new long[160,144];
    private long[,] DebugScreen = new long[255,510];
    public event EventHandler<BitmapArgs> Frame;
    public event EventHandler<BitmapArgs> DebugFrame;

    public PPUUnit(ICGBMemoryBus memory)
    {
        _Memory = memory;
        _context = new PPUContenxt(memory);
    }


    /// <summary>
    /// Update the PPU
    /// </summary>
    public void Update(int cycles)
    {
        // moves 4 dots per cicle
        // moves 2 dots per cicle double speed mode

        _TCycles += cycles;
        if(_context.EnabledLCDPPU)
        {
            switch (_context.Mode)
            {
                case LCDMode.OAM:
                    ScanObjects();
                    break;
                case LCDMode.Render:
                    DrawPixels();
                    break;
                case LCDMode.HBlank:
                    HorizontalBlank();
                    break;
                case LCDMode.VBlank:
                    VerticalBlank();
                    break;
            }
            HandleCoincidence();
        }

    }

    #region Mode execution
    /// <summary>
    /// Update vram mode 3
    /// </summary>
    private void DrawPixels()
    {
        // 18 vertical tyles
        // 20 horizontal tyles
        DrawScanLine();

        if (_TCycles >= OAM_DOTS+RENDER_DOTS)
        {
            // Reset X position
            changeSTATMode(LCDMode.HBlank);
        }

        // Add https://gbdev.io/pandocs/Rendering.html#mode-3-length
        if (_context.BGWindowPriority && _context.LCDY == 0)
        {
            //Background scrolling: At the very beginning of Mode 3, rendering is paused for SCX % 8 dots while the same number of pixels are discarded from the leftmost tile.
            //var skipDots = _context.SCX % 8;
            //if(_TCycles < OAM_DOTS + skipDots)
            //{
            //    return;
            //}
        }
        SendFrame();
    }

    /// <summary>
    /// Update objects mode 2
    /// </summary>
    private void ScanObjects()
    {
        if (_TCycles >= OAM_DOTS)
        {
            changeSTATMode(LCDMode.Render);
        }
        else
        {
            // Every two dots scan 1 tile

        }
    }
    /// <summary>
    /// Update VBlank mode 1
    /// </summary>
    private void VerticalBlank()
    {
        if (_TCycles >= SCANLINE_DOTS)
        {
            _context.LYCY++;
            if (_context.LCDY == SCREEN_VBLANK_HEIGHT)
            {
                changeSTATMode(LCDMode.OAM);
                _context.LYCY = 0;
                _TCycles -= SCANLINE_DOTS;
            }
        }
    }

    /// <summary>
    /// Update HBlank mode 0
    /// </summary>
    private void HorizontalBlank()
    {
        if (_TCycles >= SCANLINE_DOTS)
        {
            increment_ly();
            if(_context.LCDY >= SCREEN_HEIGHT)
            {
                changeSTATMode(LCDMode.VBlank);
                requestInterrupt(VBLANK_INTERRUPT);
            }
            else
            {
                changeSTATMode(LCDMode.OAM);
            }
            _TCycles -= SCANLINE_DOTS;
        }
    }

    private void changeSTATMode(LCDMode mode)
    {
        _context.Mode = mode;
        var interrupt = (mode) switch
        {
            (LCDMode.HBlank) => BitOps.IsBit(_context.StatusData, 3) ? LCD_INTERRUPT : -1,
            (LCDMode.OAM) => BitOps.IsBit(_context.StatusData, 5) ? LCD_INTERRUPT : -1,
            (LCDMode.VBlank) => BitOps.IsBit(_context.StatusData, 4) ? LCD_INTERRUPT : -1,
            (_) =>-1
        };
        if(interrupt > 0)
        {
            requestInterrupt((byte)interrupt);
        }

    }
    #endregion

    #region Rendering

    /// <summary>
    /// Draws each pixel for the given line _context.LCDY and the 32 tiles, each tile has 8 pixels and the pixel is set to bitmap
    /// The color is pick by the method GetColor() that returns an int
    /// </summary>
    private void DrawScanLine()
    {
        if (DebugFrame != null)
        {
            DrawScanLineDebug();
        }
    }

    private void DrawScanLineDebug()
    {
        var row = _context.LCDY;
        // Background Map
        const ushort backgroundBaseAdrress = 0x8000;
        for (int y= row; y<=255; y+=8) 
        {
            for(int x=0; x<=255; x+=8)
            {
                DrawTile(ref DebugScreen, backgroundBaseAdrress, x, y);
            }
        }
    }

    private void DrawTile(ref long[,] screen, ushort baseAddress, int x, int y)
    {
        var tileY = y / 8;
        var tileX = x / 8;
        var tileOffset = (tileY == 0) ? (tileX*2): (tileY * tileX * 2) + 32;

        var a = _Memory.Read((ushort)(baseAddress + tileOffset));
        var b = _Memory.Read((ushort)(baseAddress + tileOffset + 1));
        SetColor(b,a);
    }

    private void SetColor(byte right, byte left)
    {
        BGPalette[,] TileColorMap = new BGPalette[8, 8];

        var row = 0;
        for (int pointer = 0; pointer< 16; pointer += 2)
        {
            for (int bitPosition = 7; bitPosition >= 0; bitPosition--)
            {
                TileColorMap[row, 7 - bitPosition] = (BGPalette)BitOps.JoinBits(right, left, bitPosition);
            }
            row++;
        }

    
    }

/// <summary>
/// Generates the bitmap to be seen in the screen
/// </summary>
private void SendFrame()
    {
        Frame?.Invoke(this, new BitmapArgs(Screen));
        DebugFrame?.Invoke(this, new BitmapArgs(DebugScreen));
    }
    #endregion
    /// <summary>
    /// Handles coincidence
    /// </summary>
    private void HandleCoincidence()
    {
        if (_context.LCDY == _context.LYCY)
        { //handle coincidence Flag
            _context.StatusData = BitOps.bitSet(2, _context.StatusData);
            if (BitOps.IsBit(_context.StatusData, 6))
            {
                requestInterrupt(LCD_INTERRUPT);
            }
        }
        else
        {
            _context.StatusData = BitOps.bitClear(_context.StatusData, 0);
        }
        _Memory.Write(PPUContenxt.STATUS, _context.StatusData);
    }

    /// <summary>
    /// Handles the coincidence interrupt.
     /// </summary>
    private void requestInterrupt(byte b)
    {
        var IF = _Memory.Read(0xFF0F);
        IF = BitOps.bitSet(IF,b);
        _Memory.Write(0xFF0F, IF);
    }

    private void increment_ly()
    {
        _context.LYCY++;

        if (_context.LCDY == _context.LYCY)
        {
            _context.StatusData = BitOps.bitSet(_context.StatusData,7);

            if (BitOps.IsBit(_context.StatusData,2))
            {
                requestInterrupt(LCD_INTERRUPT);
            }
        }
        else
        {
            _context.StatusData = BitOps.bitClear(_context.StatusData, 7);
        }
    }
}

internal enum RenderMode
{
    RenderBackGround,
    RenderWindow,
    RenderSprites
}

public class BitmapArgs : EventArgs
{
    public readonly long[,] Image;
    public BitmapArgs(long[,] image)
    {
        Image = image;
    }
}