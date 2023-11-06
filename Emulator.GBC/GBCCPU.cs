using Emulator.Domain;
using System.Reflection.Emit;
using System.Text;

namespace Emulator.GBC;

public class GBCCPU : CPU
{
    private GBCMemory MEMORY => (GBCMemory)Hardware.Memory;
    public GBCCPU()
    {
        Hardware.Memory = new GBCMemory();
        Registers = new Registers();
        Registers.A = 0x11;
        Registers.F = 0;
        Registers.ZeroFlag = true;
        Registers.BC.Word = 0x00;
        Registers.D = 0xFF;
        Registers.E = 0x56;
        Registers.H = 0x00;
        Registers.L = 0x0D;
        Registers.PC.Word = 0x0100;
        Registers.SP = 0xFFFE;
    }
    // TODO: Implement interrupts
    public bool IME { get; private set; }
    public bool HALTED { get; private set; }

    public override void Execute()
    {
        if(Registers.PC.Word > 0x7FFF)
        {
            Console.WriteLine($"Something went wrong, reading instruction out of ROM {Registers.PC.Word.ToString("x4")}");
            return;
        }
        var opCode = Hardware.Memory.Read(this.Registers.PC.Word);
        
        Console.WriteLine(this);
        this.Registers.PC++;
        switch (opCode)
        {
            case 0x00: break; // NOP . 1 4 . - - - -
            case 0x01: Registers.BC.Word = LD16(); break; // LD BC,d16 . 3 12 . - - - -
            case 0x02: LD16(Registers.BC.Word, Registers.A); break; // LD (BC), A . 1 8 . - - - -
            case 0x03: Registers.BC++;  break; // INC BC . 1 8 . - - - -
            case 0x04: Registers.B = INC(Registers.B, 1); break; // INC B . 1 4 . Z 0 H -
            case 0x05: Registers.B = DEC(Registers.B, 1); break; // DEC B . 1 4 . Z 1 H -
            case 0x06: Registers.B = LD8(Registers.PC.Word); Registers.PC++; break; // LD B,d8 . 2 8 . - - - -
            case 0x07: Registers.A = RLCA(Registers.A); break; // RLCA . 1 4 . 0 0 0 C
            case 0x08: LD16(Registers.SP); break; // LD (a16),SP . 3 20 . - - - -
            case 0x09: Registers.HL.Word = ADD(Registers.HL.Word, Registers.BC.Word); break; // ADD HL, BC . 1 8 . - 0 H C
            case 0x0A: Registers.A = LD8(Registers.BC.Word); break; // LD A, (BC) . 1 8 . - - - -
            case 0x0B: Registers.BC.Word--; break; // DEC BC . 1 8 . - - - -
            case 0x0C: Registers.C = INC(Registers.C, 1); break; // INC C . 1 8 . Z 0 H -
            case 0x0D: Registers.C = DEC(Registers.C, 1); break; // DEC . 1 4 . Z 1 H
            case 0x0E: Registers.C = LD8(Registers.PC.Word); Registers.PC++; break; // LD C,d8 . 2 8 . - - - -
            case 0x0F: Registers.A = RRCA(Registers.A); break; // RRCA . 1 4 . 0 0 0 C
            case 0x10: STOP(); break; // STOP 0 . 2 4 . - - - -
            case 0x11: Registers.DE.Word = LD16();  break; // LD DE,d16 . 3 12 . - - - -
            case 0x12: LDP8(Registers.DE.Word, Registers.A); break; // LD (DE), A . 1 8 . - - - -
            case 0x13: Registers.DE.Word = INC(Registers.DE.Word, 1) ;  break; // INC DE . 1 8 . - - - -
            case 0x14: Registers.D = INC(Registers.D, 1); break; // INC D . 1 4 . Z 0 H -
            case 0x15: Registers.D = DEC(Registers.D, 1); break; // DEC D . 1 4 . Z 1 H -
            case 0x16: Registers.D = LD8(Registers.PC.Word); Registers.PC++; break; // LD D,d8 . 2 8 . - - - -
            case 0x17: Registers.A = RLA(Registers.A);  break; // RLA - 1 4 - 0 0 0 C
            case 0x18: JR(true);; break; // JR r8 . 2 12 . - - - -
            case 0x19: Registers.HL.Word = ADD(Registers.HL.Word, Registers.DE.Word); break; // ADD HL,DE . 1 8 . - 0 H C
            case 0x1A: Registers.A = LDP8(Registers.DE.Word); break; // LD A,(DE) . 1 8 . - - - -
            case 0x1B: Registers.DE--; break; //DEC DE . 1 8 . - - - -
            case 0x1C: Registers.E = INC(Registers.E, 1); break; // INC E . 1 4 . Z 0 H -
            case 0x1D: Registers.E = DEC(Registers.E, 1); break; // DEC E . 1 4 . Z 1 H -
            case 0x1E: Registers.E = LD8(Registers.PC.Word);Registers.PC++; break; // LD E,d8 . 2 8 . - - - -
            case 0x1F: Registers.A = RRA(Registers.A); break; // RRA . 1 4 . 0 0 0 C
            case 0X20: JR(!Registers.ZeroFlag); break; // JR NZ,r8 . 2 18/8 0 - - - -
            case 0X21: Registers.HL.Word = LD16(); break; // LD HL,d16 . 3 12 . - - - -
            case 0X22: LD8(Registers.HL.Word++, Registers.A); break; // LD (HL+),A . 1 8 . - - - -
            case 0X23: Registers.HL++; break; // INC HL . 1 8 . - - - -
            case 0X24: Registers.H = INC(Registers.H, 1); break; // INC H . 1 4 . Z 0 H -
            case 0X25: Registers.H = DEC(Registers.H, 1); break; // DEC H . 1 4 . Z 1 H -
            case 0X26: Registers.H = LD8(Registers.PC.Word); Registers.PC++; break; // LD H,d8 . 2 8 . - - - -
            case 0X27: DAA(Registers.A); break; // DAA . z 4 . Z - 0 C
            case 0X28: JR(Registers.ZeroFlag); break; // JR Z,r8 . 2 12/8 . - - - -
            case 0X29: Registers.HL.Word = ADD(Registers.HL.Word, Registers.HL.Word); break; // ADD HL,HL . 1 8 . - 0 H C
            case 0X2A: Registers.A = LD8((ushort)(Registers.HL.Word++)); break; // LD A,(HL+) . 1 8 . - - - -
            case 0X2B: Registers.HL--; break; // DEC HL . 1 8 . - - - -
            case 0X2C: Registers.L = INC(Registers.L, 1); break; // INC L . 1 4 . Z 0 H -
            case 0X2D: Registers.L = DEC(Registers.L, 1); break;// DEC L . 1 4 . Z 1 H -
            case 0X2E: Registers.L = LD8(Registers.PC.Word); Registers.PC++; break; // LD L,8 . 2 8 . - - - -
            case 0X2F: CPL(); break; // CPL 1 4 . - 1 1 -
            case 0x30: JR(!Registers.ZeroFlag); break; // JR NZ,r8 . 2 12/8 . - - - -
            case 0x31: Registers.SP= LD16(); break; // LD HL,d16 . 3 12 . - - - -
            case 0x32: Registers.A = LD8(Registers.HL.Word--); break; // LD (HL-),A . 1 8 . - - - -
            case 0x33: Registers.SP++; break; // INC SP . 1 8 . - - - -
            case 0x34: INCA(Registers.HL.Word, 1); break; // INC (HL) . 1 12 . Z 0 H -
            case 0x35: DECA(Registers.HL.Word, 1); break; // DEC (HL) . 1 12 . Z 1 H -
            case 0x36: Hardware.Memory.Write(Registers.HL.Word, LD8(Registers.PC.Word)); Registers.PC++; break; // LD (HL),d8 . 2 12 . - - - -
            case 0x37: SCF(); break; // SCF . 1 4 . - 0 0 1
            case 0x38: JR(Registers.CarryFlag); break; // JR C,r8 . 2 12/8 . - - - -
            case 0x39: Registers.HL.Word = ADD(Registers.HL.Word, Registers.SP) ;  break; // ADD HL,SP . 1 8 . - 0 H C
            case 0x3A: Registers.A = LD8(Registers.HL.Word--); break; // LD A,(HL-) . 1 8 . - - - -
            case 0x3B: Registers.SP--; break; // DEC SP . 1 8 . - - - -
            case 0x3C: Registers.A = INC(Registers.A, 1); break; // INC A . 1 4 . Z 0 H -
            case 0x3D: Registers.A = DEC(Registers.A, 1); break; // DEC A . 1 4 . Z 0 H -
            case 0x3E: Registers.A = LD8(Registers.PC.Word); Registers.PC++; break; // LD A,d8 . 2 8 . - - - -
            case 0x3F: CCF(); break; // CCF . 1 4 . - 0 0 C
            case 0x40: Registers.B = Registers.B; break; // LD B,B . 1 4 . - - - -
            case 0x41: Registers.B = Registers.C; break; // LD B,C . 1 4 . - - - -
            case 0x42: Registers.B = Registers.D; break; // LD B,D . 1 4 . - - - -
            case 0x43: Registers.B = Registers.E; break; // LD B,E . 1 4 . - - - -
            case 0x44: Registers.B = Registers.H; break; // LD B,H . 1 4 . - - - -
            case 0x45: Registers.B = Registers.L; break; // LD B,L . 1 4 . - - - -
            case 0x46: Registers.B = LD8(Registers.HL.Word); break; // LD B,(HL) . 1 4 . - - - -
            case 0x47: Registers.B = Registers.A; break; // LD B,A . 1 4 . - - - -
            case 0x48: Registers.C = Registers.B; break; // LD C,B . 1 4 . - - - -
            case 0x49: Registers.C = Registers.C; break; // LD C,C . 1 4 . - - - -
            case 0x4A: Registers.C = Registers.D; break; // LD C,D . 1 4 . - - - -
            case 0x4B: Registers.C = Registers.E; break; // LD C,E . 1 4 . - - - -
            case 0x4C: Registers.C = Registers.H; break; // LD C,H . 1 4 . - - - -
            case 0x4D: Registers.C = Registers.L; break; // LD C,L . 1 4 . - - - -
            case 0x4E: Registers.C = LD8(Registers.HL.Word); break; // LD C,(HL) . 1 4 . - - - -
            case 0x4F: Registers.C = Registers.A; break; // LD C,A . 1 4 . - - - -
            case 0x50: Registers.D = Registers.B; break; // LD D,B . 1 4 . - - - -
            case 0x51: Registers.D = Registers.C; break; // LD D,C . 1 4 . - - - -
            case 0x52: Registers.D = Registers.D; break; // LD D,D . 1 4 . - - - -
            case 0x53: Registers.D = Registers.E; break; // LD D,E . 1 4 . - - - -
            case 0x54: Registers.D = Registers.H; break; // LD D,H . 1 4 . - - - -
            case 0x55: Registers.D = Registers.L; break; // LD D,L . 1 4 . - - - -
            case 0x56: Registers.D = LD8(Registers.HL.Word); break; // LD D,(HL) . 1 4 . - - - -
            case 0x57: Registers.D = Registers.A; break; // LD D,A . 1 4 . - - - -
            case 0x58: Registers.E = Registers.B; break; // LD E,B . 1 4 . - - - -
            case 0x59: Registers.E = Registers.C; break; // LD E,C . 1 4 . - - - -
            case 0x5A: Registers.E = Registers.D; break; // LD E,D . 1 4 . - - - -
            case 0x5B: Registers.E = Registers.E; break; // LD E,E . 1 4 . - - - -
            case 0x5C: Registers.E = Registers.H; break; // LD E,H . 1 4 . - - - -
            case 0x5D: Registers.E = Registers.L; break; // LD E,L . 1 4 . - - - -
            case 0x5E: Registers.E = LD8(Registers.HL.Word); break; // LD E,(HL) . 1 4 . - - - -
            case 0x5F: Registers.E = Registers.A; break; // LD E,A . 1 4 . - - - -
            case 0x60: Registers.H = Registers.B; break; // LD H,B . 1 4 . - - - -
            case 0x61: Registers.H = Registers.C; break; // LD H,C . 1 4 . - - - -
            case 0x62: Registers.H = Registers.D; break; // LD H,D . 1 4 . - - - -
            case 0x63: Registers.H = Registers.E; break; // LD H,E . 1 4 . - - - -
            case 0x64: Registers.H = Registers.H; break; // LD H,H . 1 4 . - - - -
            case 0x65: Registers.H = Registers.L; break; // LD H,L . 1 4 . - - - -
            case 0x66: Registers.H = LD8(Registers.HL.Word); break; // LD H,(HL) . 1 4 . - - - -
            case 0x67: Registers.H = Registers.A; break; // LD H,A . 1 4 . - - - -
            case 0x68: Registers.L = Registers.B; break; // LD L,B . 1 4 . - - - -
            case 0x69: Registers.L = Registers.C; break; // LD L,C . 1 4 . - - - -
            case 0x6A: Registers.L = Registers.D; break; // LD L,D . 1 4 . - - - -
            case 0x6B: Registers.L = Registers.E; break; // LD L,E . 1 4 . - - - -
            case 0x6C: Registers.L = Registers.H; break; // LD L,H . 1 4 . - - - -
            case 0x6D: Registers.L = Registers.L; break; // LD L,L . 1 4 . - - - -
            case 0x6E: Registers.L = LD8(Registers.HL.Word); break; // LD L,(HL) . 1 4 . - - - -
            case 0x6F: Registers.L = Registers.A; break; // LD L,A . 1 4 . - - - -
            case 0x70: Hardware.Memory.Write(Registers.HL.Word, Registers.B); break; // LD (HL),B . 1 8 . - - - -
            case 0x71: Hardware.Memory.Write(Registers.HL.Word, Registers.C); break; // LD (HL),C . 1 8 . - - - -
            case 0x72: Hardware.Memory.Write(Registers.HL.Word, Registers.D); break; // LD (HL),D . 1 8 . - - - -
            case 0x73: Hardware.Memory.Write(Registers.HL.Word, Registers.E); break; // LD (HL),E . 1 8 . - - - -
            case 0x74: Hardware.Memory.Write(Registers.HL.Word, Registers.H); break; // LD (HL),H . 1 8 . - - - -
            case 0x75: Hardware.Memory.Write(Registers.HL.Word, Registers.L); break; // LD (HL),L . 1 8 . - - - -
            case 0x76: HALT(); break;
            case 0x77: Registers.HL.Word = LD8(Registers.A); break; // LD (HL),L . 1 8 . - - - -
            case 0x78: Registers.A = Registers.B; break; // LD A,B . 1 4 . - - - -
            case 0x79: Registers.A =Registers.C; break; // LD A,C . 1 4 . - - - -
            case 0x7A: Registers.A =Registers.D; break; // LD A,D . 1 4 . - - - -
            case 0x7B: Registers.A =Registers.E; break; // LD A,E . 1 4 . - - - -
            case 0x7C: Registers.A =Registers.H; break; // LD A,H . 1 4 . - - - -
            case 0x7D: Registers.A =Registers.L; break; // LD A,L . 1 4 . - - - -
            case 0x7E: Registers.A = LD8(Registers.HL.Word); break; // LD A,(HL) . 1 4 . - - - -
            case 0x7F: Registers.A =Registers.A; break; // LD A,A . 1 4 . - - - -
            case 0x80: Registers.A = ADD(Registers.A, Registers.B); break; // ADD A,B . 1 4 . Z 0 H C
            case 0x81: Registers.A = ADD(Registers.A, Registers.C); break; // ADD A,C . 1 4 . Z 0 H C
            case 0x82: Registers.A = ADD(Registers.A, Registers.D); break; // ADD A,D . 1 4 . Z 0 H C
            case 0x83: Registers.A = ADD(Registers.A, Registers.E); break; // ADD A,E . 1 4 . Z 0 H C
            case 0x84: Registers.A = ADD(Registers.A, Registers.H); break; // ADD A,H . 1 4 . Z 0 H C
            case 0x85: Registers.A = ADD(Registers.A, Registers.L); break; // ADD A,L . 1 4 . Z 0 H C
            case 0x86: Registers.A = ADD(Registers.A, LD8(Registers.HL.Word)); break; // ADD A,L . 1 4 . Z 0 H C
            case 0x87: Registers.A = ADD(Registers.A, Registers.A); break; // ADD A,A . 1 4 . Z 0 H C
            case 0x88: Registers.A = ADDC(Registers.A, Registers.B); break; // ADC A,B . 1 4 . Z 0 H C
            case 0x89: Registers.A = ADDC(Registers.A, Registers.C); break; // ADC A,C . 1 4 . Z 0 H C
            case 0x8A: Registers.A = ADDC(Registers.A, Registers.D); break; // ADC A,D . 1 4 . Z 0 H C
            case 0x8B: Registers.A = ADDC(Registers.A, Registers.E); break; // ADC A,E . 1 4 . Z 0 H C
            case 0x8C: Registers.A = ADDC(Registers.A, Registers.H); break; // ADC A,H . 1 4 . Z 0 H C
            case 0x8D: Registers.A = ADDC(Registers.A, Registers.L); break; // ADC A,L . 1 4 . Z 0 H C
            case 0x8E: Registers.A = ADDC(Registers.A, LD8(Registers.HL.Word)); break; // ADC A,(HL) . 1 4 . Z 0 H C
            case 0x8F: Registers.A = ADDC(Registers.A, Registers.A); break; // ADC A,A . 1 4 . Z 0 H C
            case 0x90: Registers.A = SUB(Registers.A, Registers.B); break; // SUB B . 1 4 . Z 1 H C
            case 0x91: Registers.A = SUB(Registers.A, Registers.C); break; // SUB C . 1 4 . Z 1 H C
            case 0x92: Registers.A = SUB(Registers.A, Registers.D); break; // SUB D . 1 4 . Z 1 H C
            case 0x93: Registers.A = SUB(Registers.A, Registers.E); break; // SUB E . 1 4 . Z 1 H C
            case 0x94: Registers.A = SUB(Registers.A, Registers.H); break; // SUB H . 1 4 . Z 1 H C
            case 0x95: Registers.A = SUB(Registers.A, Registers.L); break; // SUB L . 1 4 . Z 1 H C
            case 0x96: Registers.A = SUB(Registers.A, LD8(Registers.HL.Word)); break; // SUB HL . 1 8 . Z 1 H C
            case 0x97: Registers.A = SUB(Registers.A, Registers.A); break; // SUB A . 1 4 . Z 1 H C
            case 0x98: Registers.A = SUBC(Registers.A, Registers.B); break; // SBC A,B . 1 4 . Z 4 H C
            case 0x99: Registers.A = SUBC(Registers.A, Registers.C); break; // SBC A,C . 1 4 . Z 4 H C
            case 0x9A: Registers.A = SUBC(Registers.A, Registers.D); break; // SBC A,D . 1 4 . Z 4 H C
            case 0x9B: Registers.A = SUBC(Registers.A, Registers.E); break; // SBC A,E . 1 4 . Z 4 H C
            case 0x9C: Registers.A = SUBC(Registers.A, Registers.H); break; // SBC A,H . 1 4 . Z 4 H C
            case 0x9D: Registers.A = SUBC(Registers.A, Registers.L); break; // SBC A,L . 1 4 . Z 4 H C
            case 0x9E: Registers.A = SUBC(Registers.A, LD8(Registers.HL.Word)); break; // SBC A,HL . 1 4 . Z 4 H C
            case 0x9F: Registers.A = SUBC(Registers.A, Registers.A); break; // SBC A,A . 1 4 . Z 4 H C
            case 0xA0: AND(Registers.A, Registers.B); break; // AND B . 1 4 . Z 0 1 0
            case 0xA1: AND(Registers.A, Registers.C); break; // AND C . 1 4 . Z 0 1 0
            case 0xA2: AND(Registers.A, Registers.D); break; // AND D . 1 4 . Z 0 1 0
            case 0xA3: AND(Registers.A, Registers.E); break; // AND E . 1 4 . Z 0 1 0
            case 0xA4: AND(Registers.A, Registers.H); break; // AND H . 1 4 . Z 0 1 0
            case 0xA5: AND(Registers.A, Registers.L); break; // AND L . 1 4 . Z 0 1 0
            case 0xA6: AND(Registers.A, LD8(Registers.HL.Word)); break; // AND HL . 1 4 . Z 0 1 0
            case 0xA7: AND(Registers.A, Registers.A); break; // AND A . 1 4 . Z 0 1 0
            case 0xA8: XOR(Registers.A, Registers.B); break; // XOR B . 1 4 . Z 0 0 0
            case 0xA9: XOR(Registers.A, Registers.C); break; // XOR C . 1 4 . Z 0 0 0
            case 0xAA: XOR(Registers.A, Registers.D); break; // XOR D . 1 4 . Z 0 0 0
            case 0xAB: XOR(Registers.A, Registers.E); break; // XOR E . 1 4 . Z 0 0 0
            case 0xAC: XOR(Registers.A, Registers.H); break; // XOR H . 1 4 . Z 0 0 0
            case 0xAD: XOR(Registers.A, Registers.L); break; // XOR L . 1 4 . Z 0 0 0
            case 0xAE: XOR(Registers.A, LD8(Registers.HL.Word)); break; // XOR HL . 1 4 . Z 0 0 0
            case 0xAF: XOR(Registers.A, Registers.A); break; // XOR A . 1 4 . Z 0 0 
            case 0xB0: OR(Registers.A, Registers.B); break; // OR B . 1 4 . Z 0 0 0 
            case 0xB1: OR(Registers.A, Registers.C); break; // OR C . 1 4 . Z 0 0 0 
            case 0xB2: OR(Registers.A, Registers.D); break; // OR D . 1 4 . Z 0 0 0 
            case 0xB3: OR(Registers.A, Registers.E); break; // OR E . 1 4 . Z 0 0 0 
            case 0xB4: OR(Registers.A, Registers.H); break; // OR H . 1 4 . Z 0 0 0 
            case 0xB5: OR(Registers.A, Registers.L); break; // OR L . 1 4 . Z 0 0 0 
            case 0xB6: OR(Registers.A, LD8(Registers.HL.Word)); break; // OR HL . 1 4 . Z 0 0 0 
            case 0xB7: OR(Registers.A, Registers.A); break; // OR A . 1 4 . Z 0 0 0 
            case 0xB8: CP(Registers.A, Registers.B); break; // CP B . 1 4 . Z 1 H C 
            case 0xB9: CP(Registers.A, Registers.C); break; // CP C . 1 4 . Z 1 H C 
            case 0xBA: CP(Registers.A, Registers.D); break; // CP D . 1 4 . Z 1 H C 
            case 0xBB: CP(Registers.A, Registers.E); break; // CP E . 1 4 . Z 1 H C 
            case 0xBC: CP(Registers.A, Registers.H); break; // CP H . 1 4 . Z 1 H C 
            case 0xBD: CP(Registers.A, Registers.L); break; // CP L . 1 4 . Z 1 H C 
            case 0xBE: CP(Registers.A, LD8(Registers.HL.Word)); break; // CP HL . 1 4 . Z 1 H C 
            case 0xBF: CP(Registers.A, Registers.A); break; // CP A . 1 4 . Z 1 H C 
            case 0xC0: RET(!Registers.SubstractFlag); break; // RET NZ . 1 20/8 . - - - -
            case 0xC1: Registers.BC.Word = POP(); break; // POP BC . 1 12 .
            case 0xC2: JP(!Registers.ZeroFlag); break; // JP NZ,a16 . 3 16/12 . - - - -
            case 0xC3: JP(true); break; // JP a16 . 3 16 . - - - -
            case 0xC4: CALL(!Registers.ZeroFlag); break; // CALL NZ,a16 . 3 24/12 . - - - -
            case 0xC5: PUSH(Registers.BC.High, Registers.BC.Low); break; // PUSH BC . 1 16 . - - - -
            case 0xC6: Registers.A = ADD(Registers.A, LD8(Registers.PC.Word)); Registers.PC++; break; // ADD A,d8 . 2 8 . Z 0 H C
            case 0xC7: RST(0x0);  break; // RST 00H . 1 16 . - - - -
            case 0xC8: RET(Registers.ZeroFlag); break; // RET Z . 1 20/8 . - - - -
            case 0xC9: RET(true); break; // RET . 1 16 . - - - -
            case 0xCA: JP(Registers.ZeroFlag); break; // JP Z,a16 . 3 16/12 . - - - -
            case 0xCB: PREFIX_CB(LD8(Registers.PC.Word++)); break; // PREFIX CB . 1 4 . - - - -
            case 0xCC: CALL(Registers.ZeroFlag); break; // CALL Z,a16 3 24/12 . - - - -
            case 0xCD: CALL(true); break; // CALL . 3 24 . - - - -
            case 0xCE: Registers.A = ADD(Registers.A,LD8(Registers.PC.Word)); Registers.PC++; break; // ADD A,d8 . 2 8 . Z 0 H C
            case 0xCF: RST(0x8); break; // RST 08H . 1 16 . - - - -
            case 0xD0: RET(!Registers.ZeroFlag); break; // RET NC . 1 20/8 . - - - -
            case 0xD1: Registers.BC.Word = POP(); break; // POP BC . 1 12 . - - - -
            case 0xD2: JP(!Registers.ZeroFlag); break; // JP NZ,a16 . 3 16/12 . - - - -
            //case 0xD3: break;
            case 0xD4: CALL(!Registers.CarryFlag); break; // CALL NC,a16 . 3 24/12 - - - -
            case 0xD5: PUSH(Registers.DE.High, Registers.DE.Low); break; // PUSH DE . 1 16 . - - - -
            case 0xD6: Registers.A = SUB(Registers.A, LD8(Registers.PC.Word)); Registers.PC++; break; // SUB d8 . 2 8 . Z 1 H C
            case 0xD7: RST(0x10); break; // RST 10H . 1 16 . - - - -
            case 0xD8: RET(Registers.CarryFlag); break; // RET Z . 1 20/8 . - - - -
            case 0xD9: RET(true); IME = true; break; // RETI . 1 16 . - - - -
            case 0xDA: JP(Registers.CarryFlag); break; // JP C,a16 . 3 16/12 . - - - -
            //case 0xDB: break;
            case 0xDC: CALL(Registers.CarryFlag); break; // CALL C,a16 . 3 24/12 . - - - -
            //case 0xDD: break; 
            case 0xDE: Registers.A = SBC8(Registers.A, LD8(Registers.PC.Word)); Registers.PC++; break; // SBC A,d8 . 2 8 . Z 1 H C
            case 0xDF: RST(0x18); break; // RST 18H . 1 16 . - - - -
            case 0xE0: LDHA8(Registers.PC.Word, Registers.A); Registers.PC++;  break; // LDH (a8),A . 2 12 . - - - -
            case 0xE1: Registers.HL.Word = POP(); break; // POP HL . 1 12 . - - - -
            case 0xE2: LDH8(Registers.C, Registers.A); break; // LD (C),A . 2 8 . - - - -
            //case 0xE3: break;
            //case 0xE4: break;
            case 0xE5: PUSH(Registers.HL.High, Registers.HL.Low); break; // PUSH HL . 1 16 . - - - -
            case 0xE6: Registers.A = AND(Registers.A, LD8(Registers.PC.Word)); Registers.PC++; break; // AND d8 . 2 8 . Z 0 1 0
            case 0xE7: RST(0X20); break; // RST 20H .. 1 16 . - - - -
            case 0xE8: Registers.SP = ADDr8(Registers.SP); break; // ADD SP,r8 . 2 16 . 0 0 H C
            case 0xE9: Registers.PC = Registers.HL; break; // JP (HL) . 1 4 . - - - -
            case 0xEA: LD16(Registers.A); break; // LD (a16),A . 3 16 . - - - -
            //case 0xEB: break;
            //case 0xEC: break;
            //case 0xED: break;
            case 0xEE: XOR(Registers.A, LD8(Registers.PC.Word)); Registers.PC++; break; // XOR d8 . 2 8 . Z 0 0 0
            case 0xEF: RST(0X28); break; // RST 28H . 1 16 . - - - -
            case 0xF0: Registers.A = LDHP8(Registers.PC.Word); Registers.PC++; break; // LDH A,(a8) . 2 12 . - - - -
            case 0xF1: Registers.AF.Word = POP(); break; // POP AF . 1 12 . Z N H C
            case 0xF2: Registers.A = LDHA8(Registers.C); break; // LD A,(C) . 2 8 . - - - -
            case 0xF3: IME = false; break; // DI . 1 4 . - - - -
            //case 0xF4: break;
            case 0xF5: PUSH(Registers.AF.High, Registers.AF.Low); break; // PUSH AF . 1 16 . - - - -
            case 0xF6: Registers.A = OR(Registers.A, LD8(Registers.PC.Word)); Registers.PC++; break; // OR d8 . 2 8 . Z 0 0 0
            case 0xF7: RST(0x30); break; // RST 30H . 1 16 . - - - -
            case 0xF8: Registers.HL.Word = ADDr8(Registers.SP); break; //LD HL,SP+R8 . 2 12 .0 0 H C
            case 0xF9: Registers.SP = Registers.HL.Word; break; // LD SP,HL . 1 8 . - - - -
            case 0xFA: Registers.A = LDP16(); break; // LD A,(a16) . 3 16 . - - - -
            case 0xFB: IME = true; break; // EI . 1 8 . - - - -
            //case 0xFC: break;
            //case 0xFD: break;
            case 0xFE: CP(Registers.A, LD8(Registers.PC.Word)); Registers.PC++; break; // CP d8 . 2 8 . Z 1 H C
            case 0xFF: RST(0x38); break; // RST 38H . 1 16 . - - - -
            default: Console.WriteLine($"PC {Registers.PC.Word} OPCode {opCode.ToString("X2")} not implemented"); break;
        }
    }

    private void PREFIX_CB(ushort opCode)
    {
        switch (opCode)
        {
            case 0x00: Registers.B = RLC(Registers.B); break; // RLC B . 2 8 . Z 0 0 C
            case 0x01: Registers.C = RLC(Registers.C); break; // RLC C . 2 8 . Z 0 0 C
            case 0x02: Registers.D = RLC(Registers.D); break; // RLC D . 2 8 . Z 0 0 C
            case 0x03: Registers.E = RLC(Registers.E); break; // RLC E . 2 8 . Z 0 0 C
            case 0x04: Registers.H = RLC(Registers.H); break; // RLC H . 2 8 . Z 0 0 C
            case 0x05: Registers.L = RLC(Registers.L); break; // RLC L . 2 8 . Z 0 0 C
            case 0x06: LD8(Registers.HL.Word, RLC(LD8(Registers.HL.Word))); break; // RLC HL . 2 16 . Z 0 0 C
            case 0x07: Registers.A = RLC(Registers.A); break; // RLC A . 2 8 . Z 0 0 C
            case 0x08: Registers.B = RRC(Registers.B); break; // RRC B . 2 8 . Z 0 0 C
            case 0x09: Registers.C = RRC(Registers.C); break; // RRC C . 2 8 . Z 0 0 C
            case 0x0A: Registers.D = RRC(Registers.D); break; // RRC D . 2 8 . Z 0 0 C
            case 0x0B: Registers.E = RRC(Registers.E); break; // RRC E . 2 8 . Z 0 0 C
            case 0x0C: Registers.H = RRC(Registers.H); break; // RRC H . 2 8 . Z 0 0 C
            case 0x0D: Registers.L = RRC(Registers.L); break; // RRC L . 2 8 . Z 0 0 C
            case 0x0E: LD8(Registers.HL.Word, RRC(LD8(Registers.HL.Word))); break; // RRC HL . 2 16 . Z 0 0 C
            case 0x0F: Registers.A = RRC(Registers.A); break; // RRC A . 2 8 . Z 0 0 C
            case 0x10: Registers.B = RL(Registers.B); break; // RL B . 2 8 . Z 0 0 C
            case 0x11: Registers.C = RL(Registers.C); break; // RL C . 2 8 . Z 0 0 C
            case 0x12: Registers.D = RL(Registers.D); break; // RL D . 2 8 . Z 0 0 C
            case 0x13: Registers.E = RL(Registers.E); break; // RL E . 2 8 . Z 0 0 C
            case 0x14: Registers.H = RL(Registers.H); break; // RL H . 2 8 . Z 0 0 C
            case 0x15: Registers.L = RL(Registers.L); break; // RL L . 2 8 . Z 0 0 C
            case 0x16: LD8(Registers.HL.Word, RL(LD8(Registers.HL.Word))); break; // RL HL . 2 16 . Z 0 0 C
            case 0x17: Registers.A = RL(Registers.A); break; // RL A . 2 8 . Z 0 0 C
            case 0x18: Registers.B = RR(Registers.B); break; // RR B . 2 8 . Z 0 0 C
            case 0x19: Registers.C = RR(Registers.C); break; // RR C . 2 8 . Z 0 0 C
            case 0x1A: Registers.D = RR(Registers.D); break; // RR D . 2 8 . Z 0 0 C
            case 0x1B: Registers.E = RR(Registers.E); break; // RR E . 2 8 . Z 0 0 C
            case 0x1C: Registers.H = RR(Registers.H); break; // RR H . 2 8 . Z 0 0 C
            case 0x1D: Registers.L = RR(Registers.L); break; // RR L . 2 8 . Z 0 0 C
            case 0x1E: LD8(Registers.HL.Word, RR(LD8(Registers.HL.Word))); break; // RR HL . 2 16 . Z 0 0 C
            case 0x1F: Registers.A = RR(Registers.A); break; // RR A . 2 8 . Z 0 0 C
            case 0x20: break;
            case 0x21: break;
            case 0x22: break;
            case 0x23: break;
            case 0x24: break;
            case 0x25: break;
            case 0x26: break;
            case 0x27: break;
            case 0x28: break;
            case 0x29: break;
            case 0x2A: break;
            case 0x2B: break;
            case 0x2C: break;
            case 0x2D: break;
            case 0x2E: break;
            case 0x2F: break;
            case 0x30: break;
            case 0x31: break;
            case 0x32: break;
            case 0x33: break;
            case 0x34: break;
            case 0x35: break;
            case 0x36: break;
            case 0x37: break;
            case 0x38: break;
            case 0x39: break;
            case 0x3A: break;
            case 0x3B: break;
            case 0x3C: break;
            case 0x3D: break;
            case 0x3E: break;
            case 0x3F: break;
            case 0x40: break;
            case 0x41: break;
            case 0x42: break;
            case 0x43: break;
            case 0x44: break;
            case 0x45: break;
            case 0x46: break;
            case 0x47: break;
            case 0x48: break;
            case 0x49: break;
            case 0x4A: break;
            case 0x4B: break;
            case 0x4C: break;
            case 0x4D: break;
            case 0x4E: break;
            case 0x4F: break;
            case 0x50: break;
            case 0x51: break;
            case 0x52: break;
            case 0x53: break;
            case 0x54: break;
            case 0x55: break;
            case 0x56: break;
            case 0x57: break;
            case 0x58: break;
            case 0x59: break;
            case 0x5A: break;
            case 0x5B: break;
            case 0x5C: break;
            case 0x5D: break;
            case 0x5E: break;
            case 0x5F: break;
            case 0x60: break;
            case 0x61: break;
            case 0x62: break;
            case 0x63: break;
            case 0x64: break;
            case 0x65: break;
            case 0x66: break;
            case 0x67: break;
            case 0x68: break;
            case 0x69: break;
            case 0x6A: break;
            case 0x6B: break;
            case 0x6C: break;
            case 0x6D: break;
            case 0x6E: break;
            case 0x6F: break;
            case 0x70: break;
            case 0x71: break;
            case 0x72: break;
            case 0x73: break;
            case 0x74: break;
            case 0x75: break;
            case 0x76: break;
            case 0x77: break;
            case 0x78: break;
            case 0x79: break;
            case 0x7A: break;
            case 0x7B: break;
            case 0x7C: break;
            case 0x7D: break;
            case 0x7E: break;
            case 0x7F: break;
            case 0x80: break;
            case 0x81: break;
            case 0x82: break;
            case 0x83: break;
            case 0x84: break;
            case 0x85: break;
            case 0x86: break;
            case 0x87: break;
            case 0x88: break;
            case 0x89: break;
            case 0x8A: break;
            case 0x8B: break;
            case 0x8C: break;
            case 0x8D: break;
            case 0x8E: break;
            case 0x8F: break;
            case 0x90: break;
            case 0x91: break;
            case 0x92: break;
            case 0x93: break;
            case 0x94: break;
            case 0x95: break;
            case 0x96: break;
            case 0x97: break;
            case 0x98: break;
            case 0x99: break;
            case 0x9A: break;
            case 0x9B: break;
            case 0x9C: break;
            case 0x9D: break;
            case 0x9E: break;
            case 0x9F: break;
            case 0xA0: break;
            case 0xA1: break;
            case 0xA2: break;
            case 0xA3: break;
            case 0xA4: break;
            case 0xA5: break;
            case 0xA6: break;
            case 0xA7: break;
            case 0xA8: break;
            case 0xA9: break;
            case 0xAA: break;
            case 0xAB: break;
            case 0xAC: break;
            case 0xAD: break;
            case 0xAE: break;
            case 0xAF: break;
            case 0xB0: break;
            case 0xB1: break;
            case 0xB2: break;
            case 0xB3: break;
            case 0xB4: break;
            case 0xB5: break;
            case 0xB6: break;
            case 0xB7: break;
            case 0xB8: break;
            case 0xB9: break;
            case 0xBA: break;
            case 0xBB: break;
            case 0xBC: break;
            case 0xBD: break;
            case 0xBE: break;
            case 0xBF: break;
            case 0xC0: break;
            case 0xC1: break;
            case 0xC2: break;
            case 0xC3: break;
            case 0xC4: break;
            case 0xC5: break;
            case 0xC6: break;
            case 0xC7: break;
            case 0xC8: break;
            case 0xC9: break;
            case 0xCA: break;
            case 0xCB: break;
            case 0xCC: break;
            case 0xCD: break;
            case 0xCE: break;
            case 0xCF: break;
            case 0xD0: break;
            case 0xD1: break;
            case 0xD2: break;
            case 0xD3: break;
            case 0xD4: break;
            case 0xD5: break;
            case 0xD6: break;
            case 0xD7: break;
            case 0xD8: break;
            case 0xD9: break;
            case 0xDA: break;
            case 0xDB: break;
            case 0xDC: break;
            case 0xDD: break;
            case 0xDE: break;
            case 0xDF: break;
            case 0xE0: break;
            case 0xE1: break;
            case 0xE2: break;
            case 0xE3: break;
            case 0xE4: break;
            case 0xE5: break;
            case 0xE6: break;
            case 0xE7: break;
            case 0xE8: break;
            case 0xE9: break;
            case 0xEA: break;
            case 0xEB: break;
            case 0xEC: break;
            case 0xED: break;
            case 0xEE: break;
            case 0xEF: break;
            case 0xF0: break;
            case 0xF1: break;
            case 0xF2: break;
            case 0xF3: break;
            case 0xF4: break;
            case 0xF5: break;
            case 0xF6: break;
            case 0xF7: break;
            case 0xF8: break;
            case 0xF9: break;
            case 0xFA: break;
            case 0xFB: break;
            case 0xFC: break;
            case 0xFD: break;
            case 0xFE: break;
            case 0xFF: break;
            default: Console.WriteLine($"PC {Registers.PC.Word} OPCode {opCode.ToString("X2")} not implemented"); break;
        }

    }

    internal virtual void HALT()
    {
        // TODO: Implement
        if (!IME)
        {
            if((MEMORY.IE & MEMORY.IF) != 0){
                HALTED = true;
            }
        }
    }

    public override string ToString()
    {
        var pc = LD8((ushort)(Registers.PC.Word)).ToString("X2");
        var pc1 = LD8((ushort)(Registers.PC.Word + 1)).ToString("X2");
        var pc2 = LD8((ushort)(Registers.PC.Word + 2)).ToString("X2");
        var pc3 = LD8((ushort)(Registers.PC.Word + 3)).ToString("X2");
        return $"A: {Registers.A.ToString("X2")} F: {Registers.F.ToString("X2")} B: {Registers.B.ToString("X2")} C: {Registers.C.ToString("X2")} D: {Registers.D.ToString("X2")} E: {Registers.E.ToString("X2")} H: {Registers.H.ToString("X2")} L: {Registers.L.ToString("X2")} SP: {Registers.SP.ToString("X2")} PC: {Registers.PC.Word.ToString("X4")} ({pc} {pc1} {pc2} {pc3})";
    }
}