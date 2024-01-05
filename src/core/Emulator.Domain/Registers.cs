using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.Domain
{
    public class Registers
    {
        public Registers()
        {
            SP = 0x7FFD;
        }
        /// <summary>
        /// Acumulator
        /// </summary>
        public byte A { get => AF.High; set => AF.High = value; }
        public byte B { get => BC.High; set => BC.High = value; }
        public byte C { get => BC.Low; set => BC.Low = value; }
        public byte D { get => DE.High; set => DE.High = value; }
        public byte E { get => DE.Low; set => DE.Low = value; }
        public byte F { get => AF.Low; set => AF.Low = value; }
        public byte H { get => HL.High; set => HL.High = value; }
        public byte L { get => HL.Low; set => HL.Low = value; }


        /// <summary>
        /// Z
        /// Z - - -
        /// </summary>
        public bool ZeroFlag { get { return (F & 0x80) != 0; } set { F = value ? (byte)(F | 0x80) : (byte)(F & ~0x80); } }
        /// <summary>
        /// N
        /// - N - -
        /// </summary>
        public bool SubstractFlag { get { return (F & 0x40) != 0; } set { F = value ? (byte)(F | 0x40) : (byte)(F & ~0x40); } }
        /// <summary>
        /// H
        /// - - H - 
        /// </summary>
        public bool HaltFlag { get { return (F & 0x20) != 0; } set { F = value ? (byte)(F | 0x20) : (byte)(F & ~0x20); } }
        /// <summary>
        /// C
        /// - - - C
        /// </summary>
        public bool CarryFlag { get { return (F & 0x10) != 0; } set { F = value ? (byte)(F | 0x10) : (byte)(F & ~0x10); } }

        public Register AF { get; set; } = new();
        public Register BC { get; set; } = new();
        public Register DE { get; set; } = new();
        public Register HL { get; set; } = new();

        public ushort SP { get; set; }
        public Register PC { get; set; } = new ();
        public bool IME { get; set; }

        public override string ToString()
        {
            return $"AF({AF})BC({BC})DE({DE})HL({HL})SP({SP})PC({PC})-Z:{ZeroFlag}-N{SubstractFlag}-H{HaltFlag}-C{CarryFlag}-I{IME}";
        }
    }
}
