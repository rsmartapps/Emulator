using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.CGB.PPU
{
    internal class Tile
    {
        public byte[] Pixels { get; } = new byte[16];
    }
}
