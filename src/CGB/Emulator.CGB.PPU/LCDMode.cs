namespace Emulator.CGB.PPU;

public enum LCDMode
{
    /// <summary>
    /// Waiting until the end of the scanline
    /// 376 - mode 3’s duration
    /// VRAM, OAM, CGB Palettes
    /// </summary>
    HBlank,
    /// <summary>
    /// Waiting until the next frame
    /// 4560 dots (10 scanlines)	
    /// VRAM, OAM, CGB Palettes
    /// </summary>
    VBlank,
    /// <summary>
    /// Searching for OBJs which overlap this line
    /// 80 dots
    /// VRAM, CGB palettes
    /// </summary>
    OAM,
    /// <summary>
    /// Sending pixels to the LCD
    /// Between 172 and 289 dots, see below
    /// None
    /// </summary>
    Render
}
