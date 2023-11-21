using Emulator.CGB.Memory;
using Emulator.CGB.Memory.MBC;
using Emulator.Domain;
using Emulator.Domain.Tools;

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

    public PPUUnit(ICGBMemoryBus memory)
    {
        _Memory = memory;
    }

    private LCDC _LCDC = new();
    private STAT _STAT = new();
    private int _TCycles;

    public void Update(int cycles)
    {
        _TCycles += cycles;
        UpdateLCD();
        UpdateSTAT();

        if(_LCDC.On)
        {
            switch (_STAT.Mode)
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
                case LCDMode.VRAM:
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
            _STAT.LCDly++;
            _TCycles -= SCANLINE_CYCLES;
            if (_STAT.LCDly == SCREEN_VBLANK_HEIGHT)
            {
                changeSTATMode(2);
                _STAT.LCDly = 0;
            }
            _Memory.Write(STAT.LCD_LY, _STAT.LCDly);
        }
    }

    private void UpdateHBlank()
    {
        if (_TCycles >= HBLANK_CYCLES)
        {
            _STAT.LCDly++;
            _TCycles -= HBLANK_CYCLES;

            if (_STAT.LCDy == SCREEN_HEIGHT)
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
        _STAT.StatusData = (byte)(_STAT.StatusData & ~0x3);
        _Memory.Write(STAT.STATUS, _STAT.StatusData);
        //Accessing OAM - Mode 2 (80 cycles)
        if (mode == 2 && BitOps.IsBit(_STAT.StatusData, 5))
        { // Bit 5 - Mode 2 OAM Interrupt         (1=Enable) (Read/Write)
            requestInterrupt(LCD_INTERRUPT);
        }

        //case 3: //Accessing VRAM - Mode 3 (172 cycles) Total M2+M3 = 252 Cycles

        //HBLANK - Mode 0 (204 cycles) Total M2+M3+M0 = 456 Cycles
        else if (mode == 0 && BitOps.IsBit(_STAT.StatusData, 3))
        { // Bit 3 - Mode 0 H-Blank Interrupt     (1=Enable) (Read/Write)
            requestInterrupt(LCD_INTERRUPT);
        }

        //VBLANK - Mode 1 (4560 cycles - 10 lines)
        else if (mode == 1 && BitOps.IsBit(_STAT.StatusData, 4))
        { // Bit 4 - Mode 1 V-Blank Interrupt     (1=Enable) (Read/Write)
            requestInterrupt(LCD_INTERRUPT);
        }

    }
    #endregion

    #region Rendering


    private void DrawScanLine()
    {
        throw new NotImplementedException();
    }

    private void RenderFrame()
    {
        throw new NotImplementedException();
    }
    #endregion

    private void UpdateSTAT()
    {
        _STAT.StatusData = _Memory.Read(STAT.STATUS);
        _STAT.LCDy = _Memory.Read(STAT.LCD_Y);
        _STAT.LCDly = _Memory.Read(STAT.LCD_LY);
    }

    private void UpdateLCD()
    {
        _LCDC.data = _Memory.Read(LCDC.LCD);
    }

    private void HandleCoincidence()
    {
        if (_STAT.LCDy == _STAT.LCDly)
        { //handle coincidence Flag
            _STAT.StatusData = BitOps.bitSet(2, _STAT.StatusData);
            if (BitOps.IsBit(_STAT.StatusData, 6))
            {
                requestInterrupt(LCD_INTERRUPT);
            }
        }
        else
        {
            _STAT.StatusData = BitOps.bitClear(2, _STAT.StatusData);
        }
        _Memory.Write(STAT.STATUS, _STAT.StatusData);
    }

    private void requestInterrupt(byte b)
    {
        var IF = _Memory.Read(0xFF0F);
        IF = BitOps.bitSet(b, IF);
        _Memory.Write(0xFF0F, IF);
    }
}