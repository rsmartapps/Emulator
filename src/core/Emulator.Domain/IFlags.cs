using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.Domain
{
    public interface IFlags
    {    /// <summary>
         /// (byte)value == 0
         /// </summary>
         /// <param name="value"></param>
        void UpdateZeroFlag(int value);

        /// <summary>
        /// ((v1 & 0xF) + (v2 & 0xF)) >= 0xF
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        void UpdateHaltFlagCarry(byte v1, byte v2);

        /// <summary>
        /// ((w1 & 0xFFF) + (w2 & 0xFFF)) > 0xFFF
        /// </summary>
        /// <param name="value"></param>
        void UpdateHaltFlag(ushort w1, ushort w2);
        /// <summary>
        /// ((v1 & 0xF) + (v2 & 0xF)) > 0xF
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        void UpdateHaltFlag(byte v1, sbyte v2);

        /// <summary>
        /// (v1 & 0xF) < (v2 & 0xF)
        /// </summary>
        /// <param name="sumValue"></param>
        void UpdateHaltFlagSub(byte v1, byte v2);
        /// <summary>
        /// </summary>(v1 & 0xF) < ((v2 & 0xF) + (Registers.CarryFlag ? 1 : 0))
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        void UpdateHaltFlagSubCarry(byte v1, byte v2);
        /// <summary>
        /// (v >> 8) != 0
        /// </summary>
        /// <param name="v"></param>
        void UpdateCarryFlag(int v);
        /// <summary>
        /// (v & 0x1) != 0
        /// </summary>
        /// <param name="v"></param>
        void UpdateCarryFlagLeast(byte v);
        /// <summary>
        /// (v & 0x80) != 0
        /// </summary>
        /// <param name="v"></param>
        void UpdateCarryFlagMost(byte v);
    }
}
