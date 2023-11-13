using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.GBC.MBC
{
    internal class MBC0 : IMBC
    {
        protected Dictionary<ushort, byte> ROM;

        public MBC0(Dictionary<ushort, byte> rOM)
        {
            this.ROM = rOM;
        }

        public virtual byte ReadERAM(ushort addr)
        {
            return 0xFF; //MBC0 dosn't have ERAM
        }

        public virtual byte ReadLoROM(ushort addr)
        {
            return ROM[addr];
        }

        public virtual byte ReadHiROM(ushort addr)
        {
            return ROM[addr];
        }

        public void WriteERAM(ushort addr, byte value)
        {
            //MBC0 should ignore writes
        }

        public void WriteROM(ushort addr, byte value)
        {
            //MBC0 should ignore writes
        }

        public bool IsAccessible(ushort address)
        {
            return address <= 0x7FFF;
        }

        public byte Read(ushort address)
        {
            return ROM[address];
        }
    }
}
