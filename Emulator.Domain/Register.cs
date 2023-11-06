using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.Domain
{
    public class Register
    {

        private byte high;
        private byte low;
        public Register() { }
        public Register(ushort word)
        {
            Word = word;
        }
        public byte HighAsLow
        {
            get => (byte)(high >> 8);
        }

        public byte High {
            get => high;
            set => high = value;
        }
        public byte Low {
            get => low;
            set => low = value;
        }

        public ushort Word {
            get => (ushort)((high << 8) + low);
            set {
                high = (byte)((value >> 8) & 0xFF);
                low = (byte)(value & 0xFF);
            }
        }

        public static Register operator ++(Register bytes)
        {
            bytes.Word += 1;
            return bytes;
        }
        public static Register operator +(Register from, sbyte adding)
        {
            from.Word = (ushort)(from.Word + adding);
            return from;
        }

        public static Register operator +(Register from, ushort adding)
        {
            from.Word += adding;
            return from;
        }

        public static Register operator +(Register from, Register adding)
        {
            return new Register((ushort)(from.Word + adding.Word));
        }

        public static Register operator -(Register from, byte deducing)
        {
            from.Word -= deducing;
            return from;
        }


        public static Register operator -(Register from, Register adding)
        {
            return new Register((ushort)(from.Word - adding.Word));
        }
        public static Register operator --(Register from)
        {
            from.Word -= 1;
            return from;
        }

        public override string ToString()
        {
            return $"{High.ToString("X")} {Low.ToString("X")}";
        }
    }
}
