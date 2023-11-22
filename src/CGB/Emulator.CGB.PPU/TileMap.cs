using Emulator.CGB.Memory;
using Emulator.Domain.Tools;

namespace Emulator.CGB.PPU;

internal class TileMap
{
    private ICGBMemoryBus _memory { get; }

    public TileMap(ICGBMemoryBus memory)
    {
        _memory = memory;
    }

    public void GetTile(ushort address)
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
    }


}
/// <summary>
/// Tile representing 8x8 pixels
/// </summary>


internal class TileData
{
    internal BGPalette[,] TileColorMap = new BGPalette[8, 8];

    public TileData(byte[] color)
    {
        var row = 0;
        for (int pointer = 0; pointer < 16; pointer += 2)
        {
            SetColor(row, color[pointer], color[pointer + 1]);
            row++;
        }
    }

    private void SetColor(int row, byte left, byte right)
    {
        for (int bitPosition = 7; bitPosition >= 0; bitPosition--)
        {
            TileColorMap[row, 7 - bitPosition] = (BGPalette)BitOps.JoinBits(right, left, bitPosition);
        }
    }
}

internal enum BGPalette
{
    White,
    LightGray,
    DarkGray,
    Black
}
