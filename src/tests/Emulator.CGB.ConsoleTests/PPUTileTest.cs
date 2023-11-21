using Emulator.Domain.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.CGB.ConsoleTests
{
    internal class PPUTileTest
    {
        public PPUTileTest()
        {
            var rom = File.ReadAllBytes("B:\\Dev\\Emulators\\ROMs\\Pokemon - Blue Version (USA, Europe) (SGB Enhanced).gb");

            var color = rom[0x640A0..0x640B0];

            var map = new TileData(color);
        }
    }

    class TileData
    {
        internal BGPalette[,] TileColorMap = new BGPalette[8,8];

        public TileData(byte[] color)
        {
            var row = 0;
            for (int pointer=0;pointer<16;pointer +=2)
            {
                SetColor(row, color[pointer], color[pointer + 1]);
                row++;
            }
        }

        private void SetColor(int row, byte left, byte right)
        {
            for(int bitPosition = 7; bitPosition >= 0; bitPosition--)
            {
                TileColorMap[row, 7 - bitPosition] = (BGPalette)BitOps.JoinBits(right, left, bitPosition);
            }
        }
    }

    enum BGPalette
    {
        White,
        LightGray,
        DarkGray,
        Black
    }
}
