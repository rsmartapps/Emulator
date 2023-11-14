using Emulator.CGB.Memory.MBC;
using Emulator.Domain;

namespace Emulator.CGB.PPU;

public class PPUUnit : IPPU
{
    private IMBC Memory { get; }

    private LCDC _LCDC = new();
    private STAT STAT = new();

    public void Update()
    {
        UpdateLCD();
        UpdateSTAT();

    }

    private void UpdateSTAT()
    {
        STAT.StatusData = Memory.Read(STAT.STATUS);
        STAT.LCDy = Memory.Read(STAT.LCD_Y);
        STAT.LCDly = Memory.Read(STAT.LCD_LY);
    }

    private void UpdateLCD()
    {
        _LCDC.data = Memory.Read(LCDC.LCD);
    }

}

public class STAT
{
    public const ushort STATUS = 0xFF41;
    public const ushort LCD_Y = 0xFF44;
    public const ushort LCD_LY = 0xFF45;

    public byte StatusData { get; set; }
    public byte LCDy { get; set; }
    public byte LCDly { get; set; }


}
