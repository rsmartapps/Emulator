using Emulator.Domain;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace Emulator.Domain;

[DebuggerDisplay("{INFO}")]
public class CPU : IFlags
{
    public  Registers Registers = new Registers();

    public virtual void OpCodesAsSwitchCase()
    {
        List<int> opCodes = new();
    NEXT:
        if (Registers.PC.Word > 0x7FFF)
        {
            Console.WriteLine($"Something went wrong, reading instruction out of ROM {Registers.PC}");
            goto SAVE;
        }
        var opCode = Hardware.Memory.Read(this.Registers.PC.Word);
        opCodes.Add(opCode);
        this.Registers.PC++;
        goto NEXT;
    SAVE:
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("switch(opCode){");
        foreach (var opcode in opCodes.Distinct().Order())
        {
            sb.AppendLine($"case 0x{opcode.ToString("X")}: break;");
        }
        sb.AppendLine("}");
        var stream = File.Create($"b:/file.txt");
        stream.Close();
        File.WriteAllText($"b:/file.txt", sb.ToString());
    }

    public virtual void Execute()
    {
        var opCode = Hardware.Memory.Read(Registers.PC.Word);
        Registers.PC++;
        switch (opCode)
        {
            default:
                Console.WriteLine($"OPCode {opCode} not implemented");
                break;
        }
    }

    #region Logic Operations
    internal virtual byte OR(byte v1, byte v2)
    {
        var result = (byte)(v1 | v2);
        UpdateZeroFlag(result);
        Registers.SubstractFlag = false;
        Registers.HaltFlag = false;
        Registers.CarryFlag = false;
        return result;
    }
    internal virtual byte XOR(byte v1, byte v2)
    {
        var result = (byte)(v1 ^ v2);
        UpdateZeroFlag(result);
        Registers.SubstractFlag = false;
        Registers.HaltFlag = false;
        Registers.CarryFlag = false;
        return result;
    }
    internal virtual byte AND(byte v1, byte v2)
    {
        var result = (byte)(v1 & v2);
        UpdateZeroFlag(result);
        Registers.SubstractFlag = false;
        Registers.HaltFlag = true;
        Registers.CarryFlag = false;
        return result;
    }
    internal virtual void CP(byte v1, byte v2)
    {
        var result = v1 - v2;
        UpdateZeroFlag(result);
        Registers.SubstractFlag = true;
        UpdateHaltFlagSub(v1, v2);
        UpdateCarryFlag(result);
    }
    internal virtual byte RLC(byte v)
    {
        byte result = (byte)((v << 1) | (v >> 7));
        UpdateZeroFlag(result);
        Registers.SubstractFlag = false;
        Registers.HaltFlag = false;
        UpdateCarryFlagMost(v);
        return result;
    }
    internal virtual byte RRC(byte v)
    {
        byte result = (byte)((v >> 1) | (v << 7));
        UpdateZeroFlag(result);
        Registers.SubstractFlag = false;
        Registers.HaltFlag = false;
        UpdateCarryFlagLeast(v);
        return result;
    }
    internal virtual byte RL(byte v)
    {
        var prevC = Registers.CarryFlag;
        byte result = (byte)((v << 1) | (prevC ? 1 : 0));
        UpdateZeroFlag(result);
        Registers.SubstractFlag = false;
        Registers.HaltFlag = false;
        UpdateCarryFlagMost(v);
        return result;
    }
    internal virtual byte RR(byte v)
    {
        var prevC = Registers.CarryFlag;
        byte result = (byte)((v >> 1) | (prevC ? 1 : 0));
        UpdateZeroFlag(result);
        Registers.SubstractFlag = false;
        Registers.HaltFlag = false;
        UpdateCarryFlagLeast(v);
        return result;
    }

    #endregion

    #region Move commands

    /// <summary>
    /// Reads 8bits from memory (byte)
    /// </summary>
    /// <param name="address"></param>
    internal virtual byte LDP8(ushort address)
    {
        return Hardware.Memory.Read(Hardware.Memory.Read(address));
    }
    internal virtual byte LD8(ushort address) => Hardware.Memory.Read(address);
    internal virtual void LD8(ushort address, byte value) => Hardware.Memory.Write(address, value);
    internal virtual void LDP8(ushort address, byte value) => Hardware.Memory.Write(LD8(address), value);
    internal virtual byte LDHP8(ushort address) => Hardware.Memory.Read((ushort)(0xFF00 + Hardware.Memory.Read(address)));
    internal virtual void LDH8(ushort address, byte value) => Hardware.Memory.Write((ushort)(0xFF00 + Hardware.Memory.Read(address)), value);
    internal virtual void LDHA8(ushort address, byte value) => Hardware.Memory.Write((ushort)(0xFF00 + address), value);
    internal virtual byte LDHA8(ushort address) => Hardware.Memory.Read((ushort)(0xFF00 + address));
    internal virtual ushort LD16()
    {
        var low = Hardware.Memory.Read(Registers.PC.Word);
        Registers.PC++;
        var high = Hardware.Memory.Read(Registers.PC.Word);
        Registers.PC++;
        return (ushort)((high << 8) | low);
    }    
    internal virtual byte LDP16()
    {
        var low = Hardware.Memory.Read(Registers.PC.Word);
        Registers.PC++;
        var high = Hardware.Memory.Read(Registers.PC.Word);
        Registers.PC++;
        return Hardware.Memory.Read((ushort)((high << 8) | low));
    }
    internal virtual void LD16(byte value)
    {
        Hardware.Memory.Write(LD16(), value);
    }
    internal virtual void LD16(ushort value)
    {
        var low = (byte)value;
        var high = (byte)(value >> 8);
        Hardware.Memory.Write(Registers.PC.Word, low);
        Registers.PC++;
        Hardware.Memory.Write(Registers.PC.Word, high);
        Registers.PC++;
    }
    internal virtual void LD16(ushort address, byte value)
    {
        Hardware.Memory.Write(address, value);
    }
    #endregion

    #region Addings

    internal virtual ushort ADDr8(ushort v1)
    {
        var v2 = Hardware.Memory.Read(Registers.PC.Word);
        Registers.ZeroFlag = false;
        Registers.SubstractFlag = false;
        UpdateHaltFlag((byte)v1, v2);
        UpdateCarryFlag((byte)v1 + v2);
        return (ushort)(v1 + (sbyte)v2);
    }

    internal virtual byte ADDC(byte v1, byte v2)
    {
        var carry = Registers.CarryFlag ? 1 : 0;
        var result = v1 + v2 + carry;
        UpdateZeroFlag((byte)result);
        Registers.SubstractFlag = false;
        if (Registers.CarryFlag)
            UpdateHaltFlagCarry(v1, v2);
        else
            UpdateHaltFlag(v1, v2);
        return (byte)result;
    }

    internal virtual byte ADD(byte v1, byte v2)
    {
        var sumValue = (ushort)(v1 + v2);
        Registers.SubstractFlag = false;
        UpdateZeroFlag(sumValue);
        UpdateHaltFlag(v1, v2);
        UpdateCarryFlag((byte)sumValue);
        return (byte)sumValue;
    }

    internal virtual ushort ADD(ushort v1, ushort v2)
    {
        var sumValue = (v1 + v2);
        Registers.SubstractFlag = false;
        UpdateHaltFlag(v1, v2);
        UpdateCarryFlag(sumValue);
        return (ushort)sumValue;
    }
    #endregion

    #region Substractions
    internal virtual byte SBC8(byte v1, byte v2)
    {
        var value = (v1  - v2 - (Registers.CarryFlag ? 1 : 0));

        UpdateZeroFlag(value);
        Registers.SubstractFlag = true;
        UpdateHaltFlag(v1, v2);
        if (Registers.CarryFlag)
            UpdateHaltFlagSubCarry(v1, v2);
        else
            UpdateHaltFlagSub(v1, v2);
        UpdateCarryFlag(value);
        return (byte)value;
    }
    #endregion

    #region Decrement
    internal virtual byte SUBC(byte v1, byte v2)
    {
        var carry = Registers.CarryFlag ? 1 : 0;
        var result = v1 - v2 - carry;
        UpdateZeroFlag((byte)result);
        Registers.SubstractFlag = true;
        if (Registers.CarryFlag)
            UpdateHaltFlagSubCarry(v1, v2);
        else
            UpdateHaltFlagSub(v1, v2);
        UpdateCarryFlag((byte)result);
        return (byte)result;
    }
    internal virtual byte SUB(byte v1, byte v2)
    {
        var result = v1 - v2;
        UpdateZeroFlag((byte)result);
        Registers.SubstractFlag = true;
        UpdateHaltFlagSub(v1, v2);
        UpdateCarryFlag((byte)result);
        return (byte)result;
    }
    internal virtual byte DEC(byte value, byte decrement)
    {
        UpdateHaltFlag(value, (sbyte)-decrement);
        value -= decrement;
        UpdateZeroFlag(value);
        Registers.SubstractFlag = true;
        return value;
    }

    internal virtual void DECA(ushort address, byte increment)
    {
        var value = Hardware.Memory.Read(address);
        Hardware.Memory.Write(address, DEC(value, increment));
    }
    #endregion

    #region Increments
    internal virtual void INCA(ushort address, byte increment)
    {
        var value = Hardware.Memory.Read(address);
        Hardware.Memory.Write(address, INC(value, increment));
    }

    internal virtual byte INC(byte value, byte increment)
    {
        value += increment;
        UpdateHaltFlag(value, increment);
        UpdateZeroFlag(value);
        Registers.SubstractFlag = false;
        return value;
    }
    internal virtual ushort INC(ushort value, byte increment)
    {
        value += increment;
        return value;
    }
    #endregion

    #region Jumps
    internal virtual void RET(bool flag = false)
    {
        // TODO: Update Cycles
        if (flag)
        {
            Registers.PC.Word = POP();
        }
        else
        {
        }
    }
    /// <summary>
    /// Jumps backwards or fo
    /// </summary>
    /// <param name="flag"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal virtual void JR(bool flag)
    {
        // TODO: Add Cycles
        if (flag)
        {
            Registers.PC += (sbyte)Hardware.Memory.Read(Registers.PC.Word);
        }
        Registers.PC++;
    }
    /// <summary>
    /// Jump to address
    /// </summary>
    /// <param name="address"></param>
    internal virtual void JP(bool flag)
    {
        if (flag)
        {
            JUMP();
        }
        else
        {
            Registers.PC += 2;
        }
    }
    internal virtual void JUMP()
    {
        var low = Hardware.Memory.Read(Registers.PC.Word);
        Registers.PC++;
        var high = Hardware.Memory.Read(Registers.PC.Word);
        Registers.PC.Word = (ushort)((high << 8)+ low);
    }

    #endregion

    #region Others
    internal virtual ushort POP()
    {
        var low = Hardware.Memory.Read(Registers.SP);
        Registers.SP++;
        var high = Hardware.Memory.Read(Registers.SP);
        Registers.SP++;
        return (ushort)((high << 8) | low);
    }
    internal virtual void CCF()
    {
        Registers.CarryFlag = !Registers.CarryFlag;
        Registers.HaltFlag = false;
        Registers.SubstractFlag = false;
    }
    internal virtual void SCF()
    {
        Registers.CarryFlag = true;
        Registers.HaltFlag = false;
        Registers.SubstractFlag = false;
    }
    internal virtual byte DAA(byte value)
    {
        if (Registers.ZeroFlag)
        {
            if (Registers.CarryFlag)
            {
                value -= 0x60;
            }
            if(Registers.HaltFlag)
            {
                value -= 0x6;
            }
        }
        else
        {
            if (Registers.CarryFlag || value > 0x99)
            {
                value += 0x60;
                Registers.CarryFlag = true;
            }
            if(Registers.HaltFlag || (value & 0xF) > 0x9)
            {
                value += 0x6;
            }
        }
        UpdateZeroFlag(value);
        Registers.HaltFlag = false;
        return value;
    }
    internal virtual void STOP() => throw new NotImplementedException("STOP");
    internal virtual byte RRA(byte value)
    {
        Registers.ZeroFlag = false;
        Registers.SubstractFlag = false;
        Registers.HaltFlag = false;
        var prevFlag = Registers.CarryFlag;
        UpdateCarryFlagLeast(value);
        return (byte)((value >> 1) | (prevFlag ? 0x80 : 0));
    }

    internal virtual byte RRCA(byte value)
    {
        Registers.ZeroFlag = false;
        Registers.SubstractFlag = false;
        Registers.HaltFlag = false;
        UpdateCarryFlagLeast(value);
        return (byte)((value >> 1) | (value << 7));
    }
    internal virtual byte RLCA(byte value)
    {
        Registers.ZeroFlag = false;
        Registers.SubstractFlag = false;
        Registers.HaltFlag = false;
        UpdateCarryFlagMost(value);
        return (byte)((value << 1) | (value >> 7));
    }
    internal virtual byte RLA(byte value)
    {
        Registers.ZeroFlag = false;
        Registers.SubstractFlag = false;
        Registers.HaltFlag = false;
        var oldFlag = Registers.CarryFlag;
        UpdateCarryFlagMost(value);
        return (byte)((value << 1) | (oldFlag ? 1 : 0));
    }
    internal virtual void CALL(bool flag)
    {
        if(flag)
        {
            CALL();
        }
        else
        {
            Registers.PC += 2;
        }
    }

    internal virtual void CALL()
    {
        var low = Hardware.Memory.Read(Registers.PC.Word);
        Registers.PC++;
        var high = Hardware.Memory.Read(Registers.PC.Word);
        Registers.PC++;
        PUSH(Registers.PC.High, Registers.PC.Low);
        Registers.PC.Word = (ushort)((high << 8) | low);
    }
    internal virtual void CPL()
    {
        Registers.A = (byte)~Registers.A;
    }
    internal virtual void DI()
    {
        Registers.Interrupt = false;
    }

    internal virtual void RST(byte OPCode)
    {
        PUSH(Registers.PC.High, Registers.PC.Low);
        Registers.PC.Word = OPCode;
    }
    #endregion

    #region Memory and stack manipulation
    internal virtual void PUSH(byte high, byte low)
    {
        Registers.SP--;
        Hardware.Memory.Write(Registers.SP, high);
        Registers.SP--;
        Hardware.Memory.Write(Registers.SP, low);
    }
    #endregion

    #region Flags

    /// <summary>
    /// (byte)value == 0
    /// </summary>
    /// <param name="value"></param>
    public void UpdateZeroFlag(int value) => Registers.ZeroFlag = (byte)value == 0;

    /// <summary>
    /// ((v1 & 0xF) + (v2 & 0xF)) >= 0xF
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    public virtual void UpdateHaltFlagCarry(byte v1, byte v2) => Registers.HaltFlag = ((v1 & 0xF) + (v2 & 0xF)) >= 0xF;

    /// <summary>
    /// ((w1 & 0xFFF) + (w2 & 0xFFF)) > 0xFFF
    /// </summary>
    /// <param name="value"></param>
    public virtual void UpdateHaltFlag(ushort w1, ushort w2) => Registers.HaltFlag = ((w1 & 0xFFF) + (w2 & 0xFFF)) > 0xFFF;
    /// <summary>
    /// ((v1 & 0xF) + (v2 & 0xF)) > 0xF
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    public virtual void UpdateHaltFlag(byte v1, sbyte v2) => Registers.HaltFlag = ((v1 & 0xF) + (v2 & 0xF)) > 0xF;

    /// <summary>
    /// (v1 & 0xF) < (v2 & 0xF)
    /// </summary>
    /// <param name="sumValue"></param>
    public virtual void UpdateHaltFlagSub(byte v1, byte v2) => Registers.HaltFlag = (v1 & 0xF) < (v2 & 0xF);
    /// <summary>
    /// </summary>(v1 & 0xF) < ((v2 & 0xF) + (Registers.CarryFlag ? 1 : 0))
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    public virtual void UpdateHaltFlagSubCarry(byte v1, byte v2) => Registers.HaltFlag = (v1 & 0xF) < ((v2 & 0xF) + (Registers.CarryFlag ? 1 : 0));

    /// <summary>
    /// (v >> 8) != 0
    /// </summary>
    /// <param name="v"></param>
    public virtual void UpdateCarryFlag(int v) => Registers.CarryFlag = (v >> 8) != 0;
    /// <summary>
    /// (v & 0x1) != 0
    /// </summary>
    /// <param name="v"></param>
    public virtual void UpdateCarryFlagLeast(byte v) => Registers.CarryFlag = (v & 0x1) != 0;
    /// <summary>
    /// (v & 0x80) != 0
    /// </summary>
    /// <param name="v"></param>
    public virtual void UpdateCarryFlagMost(byte v) => Registers.CarryFlag = (v & 0x80) != 0;
    #endregion

    #region object overrides
    public string INFO => ToString();
    public override string ToString()
    {
        return $"AF({Registers.AF})BC({Registers.BC})DE({Registers.DE})HL({Registers.HL})SP({Registers.SP})PC({Registers.PC})-Z:{Registers.ZeroFlag}-N{Registers.SubstractFlag}-H{Registers.HaltFlag}-C{Registers.CarryFlag}-I{Registers.Interrupt}";
    }
    #endregion
}