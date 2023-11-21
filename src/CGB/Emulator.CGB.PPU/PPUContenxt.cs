using Emulator.CGB.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.CGB.PPU
{
    internal class PPUContenxt
    {
        private ICGBMemoryBus memory;

        public PPUContenxt(ICGBMemoryBus memory)
        {
            this.memory = memory;
        }
    }
}
