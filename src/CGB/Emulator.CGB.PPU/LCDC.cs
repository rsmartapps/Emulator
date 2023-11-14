using Emulator.Domain.Tools;

namespace Emulator.CGB.PPU;

public class LCDC
{
    public const ushort LCD = 0xFF40;

    public byte data { get; set; }

    public bool Enabled { 
        get
        {
            return BitOps.GetBit(data, 1);
        } 
    }
    public ushort WindowTileMapArea
    {
        get
        {
            if (BitOps.GetBit(data, 2))
                return 0x9800;
            else
                return 0x9C00;
        }
    }
    public bool WindowEnabled
    {
        get
        {
            return BitOps.GetBit(data, 3);
        }
    }
    public ushort BGWindowTileDataArea
    {
        get
        {
            if (BitOps.GetBit(data, 2))
                return 0x8800;
            else
                return 0x8000;
        }
    }
    public ushort BGTileMapArea
    {
        get
        {
            if (BitOps.GetBit(data, 2))
                return 0x9800;
            else
                return 0x9C00;
        }
    }
    public bool OBJSize
    {
        get
        {
            return BitOps.GetBit(data, 6);
        }
    }
    public bool OBJEnabled
    {
        get
        {
            return BitOps.GetBit(data, 7);
        }
    }
    public bool BGWindowEnablePriority
    {
        get
        {
            return BitOps.GetBit(data,8);
        }
    }

}