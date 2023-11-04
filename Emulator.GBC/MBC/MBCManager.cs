using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.GBC.MBC
{
    internal static class MBCManager
    {

        public static IMBC LoadROM(byte[] ROM)
        {
            IMBC mbc = null;
            var cartirdigeType = ROM[0x147];
            ushort address = 0;
            var mappedROM = new Dictionary<ushort, byte>(ROM.Length);
            foreach ( var opcode in ROM)
            {
                mappedROM[address] = opcode;
                address++;
            }
            switch (cartirdigeType)
            {
                case 0: mbc = new MBC0(mappedROM); break;
                case <= 0x03: mbc = new MBC1(mappedROM); break;
                case <= 0x06: break;
                case <= 0x13: break;
                case <= 0x1B: break;
                default:
                    Console.WriteLine($"CartRidge Type coudn't be matched {cartirdigeType.ToString("X")}-{cartirdigeType.ToString("x2")}");
                    break;
            }

            return mbc;
        }

        /*
         * Types
         *  MBC1 2MByte and/or 32KByte RAM
         *  MBC2 256KByte ROM and 512x4 bits RAM
         *  MBC3 2MByte and 64KByte RAM and Timer
         *  MBC5 8MByte and 128KByte RAM
         */
    }
}
