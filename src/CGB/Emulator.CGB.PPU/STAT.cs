using Emulator.Domain.Tools;

namespace Emulator.CGB.PPU;

public class STAT
{
    public const ushort STATUS = 0xFF41;
    public const ushort LCD_Y = 0xFF44;
    public const ushort LCD_LY = 0xFF45;

    public byte StatusData { get; set; }
    public byte LCDy { get; set; }
    public byte LCDly { get; set; }

    public bool HBlankMode_0 
    { 
        get
        {
            return !BitOps.IsBit(StatusData,1) && !BitOps.IsBit(StatusData, 0);
        } 
    }
    public bool VBlankMode_1
    {
        get
        {
            return !BitOps.IsBit(StatusData, 1) && BitOps.IsBit(StatusData, 0);
        }
    }
    public bool SearchOAMMode_2
    {
        get
        {
            return !BitOps.IsBit(StatusData, 1) && !BitOps.IsBit(StatusData, 0);
        }
    }
    public bool TransferDataMode_3
    {
        get
        {
            return !BitOps.IsBit(StatusData, 1) && !BitOps.IsBit(StatusData, 0);
        }
    }

    public LCDMode Mode
    {
        get
        {
            if (HBlankMode_0) return LCDMode.HBlank;
            if (VBlankMode_1) return LCDMode.VBlank;
            if (SearchOAMMode_2) return LCDMode.OAM;
            if (TransferDataMode_3) return LCDMode.VRAM;
            throw new NotImplementedException();
        }
    }

}
