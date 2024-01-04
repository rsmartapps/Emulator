using Emulator.CGB.Memory;
using Emulator.Domain.Tools;

namespace Emulator.CGB.PPU;

internal class TileMap
{
    private PPUContenxt _context;
    private ICGBMemoryBus _memory { get; }

    public TileMap(ICGBMemoryBus memory, PPUContenxt context)
    {
        _memory = memory;
        _context = context;
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
record Tile(int[,] Pixels);

internal enum BGPalette
{
    White,
    LightGray,
    DarkGray,
    Black
}
