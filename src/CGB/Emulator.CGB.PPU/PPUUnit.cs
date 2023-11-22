using Emulator.CGB.Memory;
using Emulator.CGB.Memory.MBC;
using Emulator.Domain;
using Emulator.Domain.Tools;
using System.Reflection.Metadata.Ecma335;

namespace Emulator.CGB.PPU;

public class PPUUnit : IPPU
{
    private const int SCREEN_WIDTH = 160;
    private const int SCREEN_HEIGHT = 144;
    private const int SCREEN_VBLANK_HEIGHT = 153;

    private const int OAM_CYCLES = 80;
    private const int VRAM_CYCLES = 172;
    private const int HBLANK_CYCLES = 204;
    private const int SCANLINE_CYCLES = 456;

    private const int VBLANK_INTERRUPT = 0;
    private const int LCD_INTERRUPT = 1;


    private ICGBMemoryBus _Memory { get; }
    private PPUContenxt _context;
    private int _TCycles;

    public PPUUnit(ICGBMemoryBus memory)
    {
        _Memory = memory;
        _context = new PPUContenxt(memory);
    }


    public void Update(int cycles)
    {
        _TCycles += cycles;
        if(_context.On)
        {
            switch (_context.Mode)
            {
                case LCDMode.HBlank:
                    UpdateHBlank();
                    break;
                case LCDMode.VBlank:
                    UpdateVBlank();
                    break;
                case LCDMode.OAM:
                    UpdateOAM();
                    break;
                case LCDMode.Render:
                    UpdateVRam();
                    break;
            }
            HandleCoincidence();
        }

    }

    #region Mode execution
    private void UpdateVRam()
    {
        if (_TCycles >= VRAM_CYCLES)
        {
            changeSTATMode(0);
            _TCycles -= VRAM_CYCLES;
        }
    }

    private void UpdateOAM()
    {
        if (_TCycles >= OAM_CYCLES)
        {
            changeSTATMode(3);
            DrawScanLine();
            _TCycles -= OAM_CYCLES;
        }
    }

    private void UpdateVBlank()
    {
        if (_TCycles >= SCANLINE_CYCLES)
        {
            _context.LYCY++;
            _TCycles -= SCANLINE_CYCLES;
            if (_context.LYCY == SCREEN_VBLANK_HEIGHT)
            {
                changeSTATMode(2);
                _context.LYCY = 0;
            }
            _Memory.Write(PPUContenxt.LYC_Y, _context.LYCY);
        }
    }

    private void UpdateHBlank()
    {
        if (_TCycles >= HBLANK_CYCLES)
        {
            _context.LYCY++;
            _TCycles -= HBLANK_CYCLES;

            if (_context.LCDY == SCREEN_HEIGHT)
            {
                changeSTATMode(1);
                requestInterrupt(VBLANK_INTERRUPT);
                RenderFrame();
            }
            else
            {
                changeSTATMode(2);
            }
        }
    }
    private void changeSTATMode(int mode)
    {
        _context.StatusData = (byte)(_context.StatusData & ~0x3);
        //Accessing OAM - Mode 2 (80 cycles)
        if (mode == 2 && BitOps.IsBit(_context.StatusData, 5))
        { // Bit 5 - Mode 2 OAM Interrupt         (1=Enable) (Read/Write)
            requestInterrupt(LCD_INTERRUPT);
        }

        //case 3: //Accessing VRAM - Mode 3 (172 cycles) Total M2+M3 = 252 Cycles

        //HBLANK - Mode 0 (204 cycles) Total M2+M3+M0 = 456 Cycles
        else if (mode == 0 && BitOps.IsBit(_context.StatusData, 3))
        { // Bit 3 - Mode 0 H-Blank Interrupt     (1=Enable) (Read/Write)
            requestInterrupt(LCD_INTERRUPT);
        }

        //VBLANK - Mode 1 (4560 cycles - 10 lines)
        else if (mode == 1 && BitOps.IsBit(_context.StatusData, 4))
        { // Bit 4 - Mode 1 V-Blank Interrupt     (1=Enable) (Read/Write)
            requestInterrupt(LCD_INTERRUPT);
        }

    }
    #endregion

    #region Rendering

    /// <summary>
    /// Prepare a full line the 32 tiles in internal memory
    /// ready to be rendered
    /// </summary>
    private void DrawScanLine()
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Generates the bitmap to be seen in the screen
    /// </summary>
    private void RenderFrame()
    {
        
        throw new NotImplementedException();
    }
    #endregion

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
            _context.StatusData = BitOps.bitClear(2, _context.StatusData);
        }
        _Memory.Write(PPUContenxt.STATUS, _context.StatusData);
    }

    private void requestInterrupt(byte b)
    {
        var IF = _Memory.Read(0xFF0F);
        IF = BitOps.bitSet(b, IF);
        _Memory.Write(0xFF0F, IF);
    }
}