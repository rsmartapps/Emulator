using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.GBC.MBC
{
    internal interface IMBC
    {
        bool IsAccessible(ushort address);
        byte Read(ushort address);
        byte ReadERAM(ushort addr);
        byte ReadHiROM(ushort addr);
        byte ReadLoROM(ushort addr);
        void WriteERAM(ushort addr, byte value);
        void WriteROM(ushort addr, byte value);
    }
}
