using Emulator.Domain;
using Emulator.CGB.Memory;
using Emulator.CGB.Memory.MBC;
using Emulator.Domain.Tools;

namespace Emulator.CGB.CPU
{
    public class CGBCPU
    {
        public const ushort ADDRESS_IE = 0xFFFF;
        public const ushort ADDRESS_IF = 0xFF0F;
        public Registers Registers { get; private set; } = new();
        public ICGBMemoryBus Ram { get; private set; }

        private int _cylcleCounter = 0;
        public bool IME
        {
            get { return Registers.IME; }
            set { Registers.IME = value; }
        }
        public byte IE
        {
            get { return Ram.Read(0xFFFF); }
            set { Ram.Write(0xFFFF, value); }
        }
        public byte IF
        {
            get { return Ram.Read(0xFF0F); }
            set { Ram.Write(0xFF0F, value); }
        }
        public bool HALTED = false;
        public bool HALT_BUG = false;

        public CGBCPU(ICGBMemoryBus ram)
        {
            if (ram == null)
                ram = new CGBMemoryBus();
            Ram = ram;
        }

        public int ProcessOperation()
        {
            _cylcleCounter = 0;

            if (HALT_BUG)
            {
                Registers.PC--;
                HALT_BUG = false;
            }
            var opCode = Ram.Read(this.Registers.PC.Word);
            this.Registers.PC++;
            Console.WriteLine(this);
            ExecuteOperation(opCode);
            return _cylcleCounter;
        }

        public void ProcessInterrupts()
        {
            if (HALTED)
            {
                Registers.PC++;
                HALTED = false;
            }
            if (IME)
            {
                for (int interrupt = 0; interrupt < 5; interrupt++)
                {
                    if ((((IE & IF) >> interrupt) & 0x1) == 1)
                    {
                        Registers.SP--;
                        Ram.Write(Registers.SP, Registers.PC.High);
                        Registers.SP--;
                        Ram.Write(Registers.SP, Registers.PC.Low);
                        Registers.PC.Word = (ushort)(0x40 + (8 * interrupt));
                        IF = BitOps.bitClear(IF, (byte)interrupt);
                        IME = false;
                        _cylcleCounter += 5;
                        return;
                    }
                }
            }
        }

        #region OPCodes
        private void ExecuteOperation(byte opCode)
        {
            switch (opCode)
            {
                case 0x00: _cylcleCounter += 4; break; // NOP . 1 4 . - - - -
                case 0x01: Registers.BC.Word = LD16(); break; // LD BC,d16 . 3 12 . - - - -
                case 0x02: LD8(Registers.BC.Word, Registers.A); break; // LD (BC), A . 1 8 . - - - -
                case 0x03: Registers.BC++; _cylcleCounter += 8; break; // INC BC . 1 8 . - - - -
                case 0x04: Registers.B = INC(Registers.B, 1); break; // INC B . 1 4 . Z 0 H -
                case 0x05: Registers.B = DEC(Registers.B, 1); break; // DEC B . 1 4 . Z 1 H -
                case 0x06: Registers.B = LD8(Registers.PC.Word); Registers.PC++; break; // LD B,d8 . 2 8 . - - - -
                case 0x07: Registers.A = RLCA(Registers.A); break; // RLCA . 1 4 . 0 0 0 C
                case 0x08: LDP16(Registers.SP); break; // LD (a16),SP . 3 20 . - - - -
                case 0x09: Registers.HL.Word = ADD(Registers.HL.Word, Registers.BC.Word); break; // ADD HL, BC . 1 8 . - 0 H C
                case 0x0A: Registers.A = LD8(Registers.BC.Word); break; // LD A, (BC) . 1 8 . - - - -
                case 0x0B: Registers.BC.Word--; _cylcleCounter += 8; break; // DEC BC . 1 8 . - - - -
                case 0x0C: Registers.C = INC(Registers.C, 1); break; // INC C . 1 4 . Z 0 H -
                case 0x0D: Registers.C = DEC(Registers.C, 1); break; // DEC . 1 4 . Z 1 H
                case 0x0E: Registers.C = LD8(Registers.PC.Word); Registers.PC++; break; // LD C,d8 . 2 8 . - - - -
                case 0x0F: Registers.A = RRCA(Registers.A); break; // RRCA . 1 4 . 0 0 0 C
                case 0x10: STOP(); _cylcleCounter += 4; break; // STOP 0 . 2 4 . - - - -
                case 0x11: Registers.DE.Word = LD16(); break; // LD DE,d16 . 3 12 . - - - -
                case 0x12: LD8(Registers.DE.Word, Registers.A); break; // LD (DE), A . 1 8 . - - - -
                case 0x13: Registers.DE.Word = INC(Registers.DE.Word, 1); break; // INC DE . 1 4 . - - - -
                case 0x14: Registers.D = INC(Registers.D, 1); break; // INC D . 1 4 . Z 0 H -
                case 0x15: Registers.D = DEC(Registers.D, 1); break; // DEC D . 1 4 . Z 1 H -
                case 0x16: Registers.D = LD8(Registers.PC.Word); Registers.PC++; break; // LD D,d8 . 2 8 . - - - -
                case 0x17: Registers.A = RLA(Registers.A); break; // RLA - 1 4 - 0 0 0 C
                case 0x18: JR(true); break; // JR r8 . 2 12 . - - - -
                case 0x19: Registers.HL.Word = ADD(Registers.HL.Word, Registers.DE.Word); break; // ADD HL,DE . 1 8 . - 0 H C
                case 0x1A: Registers.A = LD8(Registers.DE.Word); break; // LD A,(DE) . 1 8 . - - - -
                case 0x1B: Registers.DE--; _cylcleCounter += 8; break; //DEC DE . 1 8 . - - - -
                case 0x1C: Registers.E = INC(Registers.E, 1); break; // INC E . 1 4 . Z 0 H -
                case 0x1D: Registers.E = DEC(Registers.E, 1); break; // DEC E . 1 4 . Z 1 H -
                case 0x1E: Registers.E = LD8(Registers.PC.Word); Registers.PC++; break; // LD E,d8 . 2 8 . - - - -
                case 0x1F: Registers.A = RRA(Registers.A); break; // RRA . 1 4 . 0 0 0 C
                case 0X20: JR(!Registers.ZeroFlag); break; // JR NZ,r8 . 2 12/8 0 - - - -
                case 0X21: Registers.HL.Word = LD16(); break; // LD HL,d16 . 3 12 . - - - -
                case 0X22: LD8(Registers.HL.Word++, Registers.A); break; // LD (HL+),A . 1 8 . - - - -
                case 0X23: Registers.HL++; _cylcleCounter += 8; break; // INC HL . 1 8 . - - - -
                case 0X24: Registers.H = INC(Registers.H, 1); break; // INC H . 1 4 . Z 0 H -
                case 0X25: Registers.H = DEC(Registers.H, 1); break; // DEC H . 1 4 . Z 1 H -
                case 0X26: Registers.H = LD8(Registers.PC.Word); Registers.PC++; break; // LD H,d8 . 2 8 . - - - -
                case 0X27: DAA(); break; // DAA . z 4 . Z - 0 C
                case 0X28: JR(Registers.ZeroFlag); break; // JR Z,r8 . 2 12/8 . - - - -
                case 0X29: Registers.HL.Word = ADD(Registers.HL.Word, Registers.HL.Word); break; // ADD HL,HL . 1 8 . - 0 H C
                case 0X2A: Registers.A = LD8((ushort)(Registers.HL.Word++)); break; // LD A,(HL+) . 1 8 . - - - -
                case 0X2B: Registers.HL--; _cylcleCounter += 8; break; // DEC HL . 1 8 . - - - -
                case 0X2C: Registers.L = INC(Registers.L, 1); break; // INC L . 1 4 . Z 0 H -
                case 0X2D: Registers.L = DEC(Registers.L, 1); break;// DEC L . 1 4 . Z 1 H -
                case 0X2E: Registers.L = LD8(Registers.PC.Word); Registers.PC++; break; // LD L,8 . 2 8 . - - - -
                case 0X2F: CPL(); break; // CPL 1 4 . - 1 1 -
                case 0x30: JR(!Registers.CarryFlag); break; // JR NZ,r8 . 2 12/8 . - - - -
                case 0x31: Registers.SP = LD16(); break; // LD HL,d16 . 3 12 . - - - -
                case 0x32: LD8(Registers.HL.Word--, Registers.A); break; // LD (HL-),A . 1 8 . - - - -
                case 0x33: Registers.SP++; _cylcleCounter += 8; break; // INC SP . 1 8 . - - - -
                case 0x34: INCA(Registers.HL.Word, 1); break; // INC (HL) . 1 12 . Z 0 H -
                case 0x35: DECA(Registers.HL.Word, 1); break; // DEC (HL) . 1 12 . Z 1 H -
                case 0x36: LD8(Registers.HL.Word, LD8(Registers.PC.Word)); Registers.PC++; _cylcleCounter -= 4; break; // LD (HL),d8 . 2 12 . - - - -
                case 0x37: SCF(); break; // SCF . 1 4 . - 0 0 1
                case 0x38: JR(Registers.CarryFlag); break; // JR C,r8 . 2 12/8 . - - - -
                case 0x39: Registers.HL.Word = ADD(Registers.HL.Word, Registers.SP); break; // ADD HL,SP . 1 8 . - 0 H C
                case 0x3A: Registers.A = LD8(Registers.HL.Word--); break; // LD A,(HL-) . 1 8 . - - - -
                case 0x3B: Registers.SP--; _cylcleCounter += 8; break; // DEC SP . 1 8 . - - - -
                case 0x3C: Registers.A = INC(Registers.A, 1); break; // INC A . 1 4 . Z 0 H -
                case 0x3D: Registers.A = DEC(Registers.A, 1); break; // DEC A . 1 4 . Z 0 H -
                case 0x3E: Registers.A = LD8(Registers.PC.Word); Registers.PC++; break; // LD A,d8 . 2 8 . - - - -
                case 0x3F: CCF(); break; // CCF . 1 4 . - 0 0 C
                case 0x40: Registers.B = Registers.B; _cylcleCounter += 4; break; // LD B,B . 1 4 . - - - -
                case 0x41: Registers.B = Registers.C; _cylcleCounter += 4; break; // LD B,C . 1 4 . - - - -
                case 0x42: Registers.B = Registers.D; _cylcleCounter += 4; break; // LD B,D . 1 4 . - - - -
                case 0x43: Registers.B = Registers.E; _cylcleCounter += 4; break; // LD B,E . 1 4 . - - - -
                case 0x44: Registers.B = Registers.H; _cylcleCounter += 4; break; // LD B,H . 1 4 . - - - -
                case 0x45: Registers.B = Registers.L; _cylcleCounter += 4; break; // LD B,L . 1 4 . - - - -
                case 0x46: Registers.B = LD8(Registers.HL.Word); break; // LD B,(HL) . 1 8 . - - - -
                case 0x47: Registers.B = Registers.A; _cylcleCounter += 4; break; // LD B,A . 1 4 . - - - -
                case 0x48: Registers.C = Registers.B; _cylcleCounter += 4; break; // LD C,B . 1 4 . - - - -
                case 0x49: Registers.C = Registers.C; _cylcleCounter += 4; break; // LD C,C . 1 4 . - - - -
                case 0x4A: Registers.C = Registers.D; _cylcleCounter += 4; break; // LD C,D . 1 4 . - - - -
                case 0x4B: Registers.C = Registers.E; _cylcleCounter += 4; break; // LD C,E . 1 4 . - - - -
                case 0x4C: Registers.C = Registers.H; _cylcleCounter += 4; break; // LD C,H . 1 4 . - - - -
                case 0x4D: Registers.C = Registers.L; _cylcleCounter += 4; break; // LD C,L . 1 4 . - - - -
                case 0x4E: Registers.C = LD8(Registers.HL.Word); break; // LD C,(HL) . 1 8 . - - - -
                case 0x4F: Registers.C = Registers.A; _cylcleCounter += 4; break; // LD C,A . 1 4 . - - - -
                case 0x50: Registers.D = Registers.B; _cylcleCounter += 4; break; // LD D,B . 1 4 . - - - -
                case 0x51: Registers.D = Registers.C; _cylcleCounter += 4; break; // LD D,C . 1 4 . - - - -
                case 0x52: Registers.D = Registers.D; _cylcleCounter += 4; break; // LD D,D . 1 4 . - - - -
                case 0x53: Registers.D = Registers.E; _cylcleCounter += 4; break; // LD D,E . 1 4 . - - - -
                case 0x54: Registers.D = Registers.H; _cylcleCounter += 4; break; // LD D,H . 1 4 . - - - -
                case 0x55: Registers.D = Registers.L; _cylcleCounter += 4; break; // LD D,L . 1 4 . - - - -
                case 0x56: Registers.D = LD8(Registers.HL.Word); break; // LD D,(HL) . 1 8 . - - - -
                case 0x57: Registers.D = Registers.A; _cylcleCounter += 4; break; // LD D,A . 1 4 . - - - -
                case 0x58: Registers.E = Registers.B; _cylcleCounter += 4; break; // LD E,B . 1 4 . - - - -
                case 0x59: Registers.E = Registers.C; _cylcleCounter += 4; break; // LD E,C . 1 4 . - - - -
                case 0x5A: Registers.E = Registers.D; _cylcleCounter += 4; break; // LD E,D . 1 4 . - - - -
                case 0x5B: Registers.E = Registers.E; _cylcleCounter += 4; break; // LD E,E . 1 4 . - - - -
                case 0x5C: Registers.E = Registers.H; _cylcleCounter += 4; break; // LD E,H . 1 4 . - - - -
                case 0x5D: Registers.E = Registers.L; _cylcleCounter += 4; break; // LD E,L . 1 4 . - - - -
                case 0x5E: Registers.E = LD8(Registers.HL.Word); break; // LD E,(HL) . 1 8 . - - - -
                case 0x5F: Registers.E = Registers.A; _cylcleCounter += 4; break; // LD E,A . 1 4 . - - - -
                case 0x60: Registers.H = Registers.B; _cylcleCounter += 4; break; // LD H,B . 1 4 . - - - -
                case 0x61: Registers.H = Registers.C; _cylcleCounter += 4; break; // LD H,C . 1 4 . - - - -
                case 0x62: Registers.H = Registers.D; _cylcleCounter += 4; break; // LD H,D . 1 4 . - - - -
                case 0x63: Registers.H = Registers.E; _cylcleCounter += 4; break; // LD H,E . 1 4 . - - - -
                case 0x64: Registers.H = Registers.H; _cylcleCounter += 4; break; // LD H,H . 1 4 . - - - -
                case 0x65: Registers.H = Registers.L; _cylcleCounter += 4; break; // LD H,L . 1 4 . - - - -
                case 0x66: Registers.H = LD8(Registers.HL.Word); break; // LD H,(HL) . 1 8 . - - - -
                case 0x67: Registers.H = Registers.A; _cylcleCounter += 4; break; // LD H,A . 1 4 . - - - -
                case 0x68: Registers.L = Registers.B; _cylcleCounter += 4; break; // LD L,B . 1 4 . - - - -
                case 0x69: Registers.L = Registers.C; _cylcleCounter += 4; break; // LD L,C . 1 4 . - - - -
                case 0x6A: Registers.L = Registers.D; _cylcleCounter += 4; break; // LD L,D . 1 4 . - - - -
                case 0x6B: Registers.L = Registers.E; _cylcleCounter += 4; break; // LD L,E . 1 4 . - - - -
                case 0x6C: Registers.L = Registers.H; _cylcleCounter += 4; break; // LD L,H . 1 4 . - - - -
                case 0x6D: Registers.L = Registers.L; _cylcleCounter += 4; break; // LD L,L . 1 4 . - - - -
                case 0x6E: Registers.L = LD8(Registers.HL.Word); break; // LD L,(HL) . 1 8 . - - - -
                case 0x6F: Registers.L = Registers.A; _cylcleCounter += 4; break; // LD L,A . 1 4 . - - - -
                case 0x70: LD8(Registers.HL.Word, Registers.B); break; // LD (HL),B . 1 8 . - - - -
                case 0x71: LD8(Registers.HL.Word, Registers.C); break; // LD (HL),C . 1 8 . - - - -
                case 0x72: LD8(Registers.HL.Word, Registers.D); break; // LD (HL),D . 1 8 . - - - -
                case 0x73: LD8(Registers.HL.Word, Registers.E); break; // LD (HL),E . 1 8 . - - - -
                case 0x74: LD8(Registers.HL.Word, Registers.H); break; // LD (HL),H . 1 8 . - - - -
                case 0x75: LD8(Registers.HL.Word, Registers.L); break; // LD (HL),L . 1 8 . - - - -
                case 0x76: HALT(); break;
                case 0x77: LD8(Registers.HL.Word, Registers.A); break; // LD (HL),A . 1 8 . - - - -
                case 0x78: Registers.A = Registers.B; _cylcleCounter += 4; break; // LD A,B . 1 4 . - - - -
                case 0x79: Registers.A = Registers.C; _cylcleCounter += 4; break; // LD A,C . 1 4 . - - - -
                case 0x7A: Registers.A = Registers.D; _cylcleCounter += 4; break; // LD A,D . 1 4 . - - - -
                case 0x7B: Registers.A = Registers.E; _cylcleCounter += 4; break; // LD A,E . 1 4 . - - - -
                case 0x7C: Registers.A = Registers.H; _cylcleCounter += 4; break; // LD A,H . 1 4 . - - - -
                case 0x7D: Registers.A = Registers.L; _cylcleCounter += 4; break; // LD A,L . 1 4 . - - - -
                case 0x7E: Registers.A = LD8(Registers.HL.Word); break; // LD A,(HL) . 1 4 . - - - -
                case 0x7F: Registers.A = Registers.A; break; // LD A,A . 1 4 . - - - -
                case 0x80: ADD(Registers.B); break; // ADD A,B . 1 4 . Z 0 H C
                case 0x81: ADD(Registers.C); break; // ADD A,C . 1 4 . Z 0 H C
                case 0x82: ADD(Registers.D); break; // ADD A,D . 1 4 . Z 0 H C
                case 0x83: ADD(Registers.E); break; // ADD A,E . 1 4 . Z 0 H C
                case 0x84: ADD(Registers.H); break; // ADD A,H . 1 4 . Z 0 H C
                case 0x85: ADD(Registers.L); break; // ADD A,L . 1 4 . Z 0 H C
                case 0x86: ADD(LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // ADD A,L . 1 8 . Z 0 H C
                case 0x87: ADD(Registers.A); break; // ADD A,A . 1 4 . Z 0 H C
                case 0x88: ADC(Registers.B); break; // ADC A,B . 1 4 . Z 0 H C
                case 0x89: ADC(Registers.C); break; // ADC A,C . 1 4 . Z 0 H C
                case 0x8A: ADC(Registers.D); break; // ADC A,D . 1 4 . Z 0 H C
                case 0x8B: ADC(Registers.E); break; // ADC A,E . 1 4 . Z 0 H C
                case 0x8C: ADC(Registers.H); break; // ADC A,H . 1 4 . Z 0 H C
                case 0x8D: ADC(Registers.L); break; // ADC A,L . 1 4 . Z 0 H C
                case 0x8E: ADC(LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // ADC A,(HL) . 1 8 . Z 0 H C
                case 0x8F: ADC(Registers.A); break; // ADC A,A . 1 4 . Z 0 H C
                case 0x90: SUB(Registers.B); break; // SUB B . 1 4 . Z 1 H C
                case 0x91: SUB(Registers.C); break; // SUB C . 1 4 . Z 1 H C
                case 0x92: SUB(Registers.D); break; // SUB D . 1 4 . Z 1 H C
                case 0x93: SUB(Registers.E); break; // SUB E . 1 4 . Z 1 H C
                case 0x94: SUB(Registers.H); break; // SUB H . 1 4 . Z 1 H C
                case 0x95: SUB(Registers.L); break; // SUB L . 1 4 . Z 1 H C
                case 0x96: SUB(LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // SUB HL . 1 8 . Z 1 H C
                case 0x97: SUB(Registers.A); break; // SUB A . 1 4 . Z 1 H C
                case 0x98: SUBC(Registers.B); break; // SBC A,B . 1 4 . Z 4 H C
                case 0x99: SUBC(Registers.C); break; // SBC A,C . 1 4 . Z 4 H C
                case 0x9A: SUBC(Registers.D); break; // SBC A,D . 1 4 . Z 4 H C
                case 0x9B: SUBC(Registers.E); break; // SBC A,E . 1 4 . Z 4 H C
                case 0x9C: SUBC(Registers.H); break; // SBC A,H . 1 4 . Z 4 H C
                case 0x9D: SUBC(Registers.L); break; // SBC A,L . 1 4 . Z 4 H C
                case 0x9E: SUBC(LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // SBC A,HL . 1 8 . Z 4 H C
                case 0x9F: SUBC(Registers.A); break; // SBC A,A . 1 4 . Z 4 H C
                case 0xA0: AND(Registers.B); break; // AND B . 1 4 . Z 0 1 0
                case 0xA1: AND(Registers.C); break; // AND C . 1 4 . Z 0 1 0
                case 0xA2: AND(Registers.D); break; // AND D . 1 4 . Z 0 1 0
                case 0xA3: AND(Registers.E); break; // AND E . 1 4 . Z 0 1 0
                case 0xA4: AND(Registers.H); break; // AND H . 1 4 . Z 0 1 0
                case 0xA5: AND(Registers.L); break; // AND L . 1 4 . Z 0 1 0
                case 0xA6: AND(LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // AND HL . 1 8 . Z 0 1 0
                case 0xA7: AND(Registers.A); break; // AND A . 1 4 . Z 0 1 0
                case 0xA8: XOR(Registers.B); break; // XOR B . 1 4 . Z 0 0 0
                case 0xA9: XOR(Registers.C); break; // XOR C . 1 4 . Z 0 0 0
                case 0xAA: XOR(Registers.D); break; // XOR D . 1 4 . Z 0 0 0
                case 0xAB: XOR(Registers.E); break; // XOR E . 1 4 . Z 0 0 0
                case 0xAC: XOR(Registers.H); break; // XOR H . 1 4 . Z 0 0 0
                case 0xAD: XOR(Registers.L); break; // XOR L . 1 4 . Z 0 0 0
                case 0xAE: XOR(LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // XOR HL . 1 8 . Z 0 0 0
                case 0xAF: XOR(Registers.A); break; // XOR A . 1 4 . Z 0 0 
                case 0xB0: OR(Registers.B); break; // OR B . 1 4 . Z 0 0 0 
                case 0xB1: OR(Registers.C); break; // OR C . 1 4 . Z 0 0 0 
                case 0xB2: OR(Registers.D); break; // OR D . 1 4 . Z 0 0 0 
                case 0xB3: OR(Registers.E); break; // OR E . 1 4 . Z 0 0 0 
                case 0xB4: OR(Registers.H); break; // OR H . 1 4 . Z 0 0 0 
                case 0xB5: OR(Registers.L); break; // OR L . 1 4 . Z 0 0 0 
                case 0xB6: OR(LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // OR HL . 1 8 . Z 0 0 0 
                case 0xB7: OR(Registers.A); break; // OR A . 1 4 . Z 0 0 0 
                case 0xB8: CP(Registers.A, Registers.B); break; // CP B . 1 4 . Z 1 H C 
                case 0xB9: CP(Registers.A, Registers.C); break; // CP C . 1 4 . Z 1 H C 
                case 0xBA: CP(Registers.A, Registers.D); break; // CP D . 1 4 . Z 1 H C 
                case 0xBB: CP(Registers.A, Registers.E); break; // CP E . 1 4 . Z 1 H C 
                case 0xBC: CP(Registers.A, Registers.H); break; // CP H . 1 4 . Z 1 H C 
                case 0xBD: CP(Registers.A, Registers.L); break; // CP L . 1 4 . Z 1 H C 
                case 0xBE: CP(Registers.A, LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // CP HL . 1 8 . Z 1 H C 
                case 0xBF: CP(Registers.A, Registers.A); break; // CP A . 1 4 . Z 1 H C 
                case 0xC0: RET(!Registers.ZeroFlag); break; // RET NZ . 1 20/8 . - - - -
                case 0xC1: Registers.BC.Word = POP(); break; // POP BC . 1 12 .
                case 0xC2: JP(!Registers.ZeroFlag); break; // JP NZ,a16 . 3 16/12 . - - - -
                case 0xC3: JP(true); break; // JP a16 . 3 16 . - - - -
                case 0xC4: CALL(!Registers.ZeroFlag); break; // CALL NZ,a16 . 3 24/12 . - - - -
                case 0xC5: PUSH(Registers.BC.High, Registers.BC.Low); break; // PUSH BC . 1 16 . - - - -
                case 0xC6: ADD(LD8(Registers.PC.Word)); Registers.PC++; _cylcleCounter -= 4; break; // ADD A,d8 . 2 8 . Z 0 H C
                case 0xC7: RST(0x0); break; // RST 00H . 1 16 . - - - -
                case 0xC8: RET(Registers.ZeroFlag); break; // RET Z . 1 20/8 . - - - -
                case 0xC9: RET(true); _cylcleCounter -= 4; break; // RET . 1 16 . - - - -
                case 0xCA: JP(Registers.ZeroFlag); break; // JP Z,a16 . 3 16/12 . - - - -
                case 0xCB: PREFIX_CB(LD8(Registers.PC.Word++)); break; // PREFIX CB . 1 4 . - - - -
                case 0xCC: CALL(Registers.ZeroFlag); break; // CALL Z,a16 3 24/12 . - - - -
                case 0xCD: CALL(true); break; // CALL . 3 24 . - - - -
                case 0xCE: ADC(LD8(Registers.PC.Word)); Registers.PC++; _cylcleCounter -= 4; break; // ADD A,d8 . 2 8 . Z 0 H C
                case 0xCF: RST(0x8); break; // RST 08H . 1 16 . - - - -
                case 0xD0: RET(!Registers.CarryFlag); break; // RET NC . 1 20/8 . - - - -
                case 0xD1: Registers.DE.Word = POP(); break; // POP DE . 1 12 . - - - -
                case 0xD2: JP(!Registers.CarryFlag); break; // JP NZ,a16 . 3 16/12 . - - - -
                                                            //case 0xD3: break;
                case 0xD4: CALL(!Registers.CarryFlag); break; // CALL NC,a16 . 3 24/12 - - - -
                case 0xD5: PUSH(Registers.DE.High, Registers.DE.Low); break; // PUSH DE . 1 16 . - - - -
                case 0xD6: SUB(LD8(Registers.PC.Word)); Registers.PC++; _cylcleCounter -= 4; break; // SUB d8 . 2 8 . Z 1 H C
                case 0xD7: RST(0x10); break; // RST 10H . 1 16 . - - - -
                case 0xD8: RET(Registers.CarryFlag); break; // RET Z . 1 20/8 . - - - -
                case 0xD9: RET(true); IME = true; _cylcleCounter -= 4; break; // RETI . 1 16 . - - - -
                case 0xDA: JP(Registers.CarryFlag); break; // JP C,a16 . 3 16/12 . - - - -
                                                           //case 0xDB: break;
                case 0xDC: CALL(Registers.CarryFlag); break; // CALL C,a16 . 3 24/12 . - - - -
                                                             //case 0xDD: break; 
                case 0xDE: Registers.A = SBC8(Registers.A, LD8(Registers.PC.Word)); Registers.PC++; _cylcleCounter -= 4; break; // SBC A,d8 . 2 8 . Z 1 H C
                case 0xDF: RST(0x18); break; // RST 18H . 1 16 . - - - -
                case 0xE0: LDHA8(Registers.PC.Word, Registers.A); Registers.PC++; break; // LDH (a8),A . 2 12 . - - - -
                case 0xE1: Registers.HL.Word = POP(); break; // POP HL . 1 12 . - - - -
                case 0xE2: LDH8(Registers.C, Registers.A); break; // LD (C),A . 2 8 . - - - -
                                                                  //case 0xE3: break;
                                                                  //case 0xE4: break;
                case 0xE5: PUSH(Registers.HL.High, Registers.HL.Low); break; // PUSH HL . 1 16 . - - - -
                case 0xE6: AND(LD8(Registers.PC.Word)); Registers.PC++; break; // AND d8 . 2 8 . Z 0 1 0
                case 0xE7: RST(0X20); break; // RST 20H .. 1 16 . - - - -
                case 0xE8: Registers.SP = ADDr8(Registers.SP); break; // ADD SP,r8 . 2 16 . 0 0 H C
                case 0xE9: Registers.PC = Registers.HL; break; // JP (HL) . 1 4 . - - - -
                case 0xEA: LDP8(Registers.A); break; // LD (a16),A . 3 16 . - - - -
                                                     //case 0xEB: break;
                                                     //case 0xEC: break;
                                                     //case 0xED: break;
                case 0xEE: XOR(LD8(Registers.PC.Word)); Registers.PC++; break; // XOR d8 . 2 8 . Z 0 0 0
                case 0xEF: RST(0X28); break; // RST 28H . 1 16 . - - - -
                case 0xF0: Registers.A = LDHP8(Registers.PC.Word); Registers.PC++; break; // LDH A,(a8) . 2 12 . - - - -
                case 0xF1:
                    Registers.AF.Word = POP();
                    var z = Registers.ZeroFlag;
                    var s = Registers.SubstractFlag;
                    var h = Registers.HaltFlag;
                    var c = Registers.CarryFlag;
                    Registers.F = 0;
                    Registers.ZeroFlag = z;
                    Registers.SubstractFlag = s;
                    Registers.HaltFlag = h;
                    Registers.CarryFlag = c;
                    break; // POP AF . 1 12 . Z N H C
                case 0xF2: Registers.A = LDHA8(Registers.C); break; // LD A,(C) . 2 8 . - - - -
                case 0xF3: DI(); break; // DI . 1 4 . - - - -
                                               //case 0xF4: break;
                case 0xF5: PUSH(Registers.AF.High, Registers.AF.Low); break; // PUSH AF . 1 16 . - - - -
                case 0xF6: OR(LD8(Registers.PC.Word)); Registers.PC++; _cylcleCounter -= 4; break; // OR d8 . 2 8 . Z 0 0 0
                case 0xF7: RST(0x30); break; // RST 30H . 1 16 . - - - -
                case 0xF8: Registers.HL.Word = ADDr8(Registers.SP); break; //LD HL,SP+R8 . 2 12 .0 0 H C
                case 0xF9: Registers.SP = Registers.HL.Word; _cylcleCounter += 8; break; // LD SP,HL . 1 8 . - - - -
                case 0xFA: Registers.A = LDP16(); break; // LD A,(a16) . 3 16 . - - - -
                case 0xFB: EI(); break; // EI . 1 8 . - - - -
                                        //case 0xFC: break;
                                        //case 0xFD: break;
                case 0xFE: CP(Registers.A, LD8(Registers.PC.Word)); Registers.PC++; _cylcleCounter -= 4; break; // CP d8 . 2 8 . Z 1 H C
                case 0xFF: RST(0x38); break; // RST 38H . 1 16 . - - - -
                                             //default: Console.WriteLine($"PC {Registers.PC.Word} OPCode {opCode.ToString("X2")} not implemented"); break;
            }
        }

        private void PREFIX_CB(ushort opCode)
        {
            _cylcleCounter -= 4;
            switch (opCode)
            {
                case 0x00: Registers.B = RLC(Registers.B); break; // RLC B . 2 8 . Z 0 0 C
                case 0x01: Registers.C = RLC(Registers.C); break; // RLC C . 2 8 . Z 0 0 C
                case 0x02: Registers.D = RLC(Registers.D); break; // RLC D . 2 8 . Z 0 0 C
                case 0x03: Registers.E = RLC(Registers.E); break; // RLC E . 2 8 . Z 0 0 C
                case 0x04: Registers.H = RLC(Registers.H); break; // RLC H . 2 8 . Z 0 0 C
                case 0x05: Registers.L = RLC(Registers.L); break; // RLC L . 2 8 . Z 0 0 C
                case 0x06: LD8(Registers.HL.Word, RLC(LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // RLC HL . 2 16 . Z 0 0 C
                case 0x07: Registers.A = RLC(Registers.A); break; // RLC A . 2 8 . Z 0 0 C
                case 0x08: Registers.B = RRC(Registers.B); break; // RRC B . 2 8 . Z 0 0 C
                case 0x09: Registers.C = RRC(Registers.C); break; // RRC C . 2 8 . Z 0 0 C
                case 0x0A: Registers.D = RRC(Registers.D); break; // RRC D . 2 8 . Z 0 0 C
                case 0x0B: Registers.E = RRC(Registers.E); break; // RRC E . 2 8 . Z 0 0 C
                case 0x0C: Registers.H = RRC(Registers.H); break; // RRC H . 2 8 . Z 0 0 C
                case 0x0D: Registers.L = RRC(Registers.L); break; // RRC L . 2 8 . Z 0 0 C
                case 0x0E: LD8(Registers.HL.Word, RRC(LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // RRC HL . 2 16 . Z 0 0 C
                case 0x0F: Registers.A = RRC(Registers.A); break; // RRC A . 2 8 . Z 0 0 C
                case 0x10: Registers.B = RL(Registers.B); break; // RL B . 2 8 . Z 0 0 C
                case 0x11: Registers.C = RL(Registers.C); break; // RL C . 2 8 . Z 0 0 C
                case 0x12: Registers.D = RL(Registers.D); break; // RL D . 2 8 . Z 0 0 C
                case 0x13: Registers.E = RL(Registers.E); break; // RL E . 2 8 . Z 0 0 C
                case 0x14: Registers.H = RL(Registers.H); break; // RL H . 2 8 . Z 0 0 C
                case 0x15: Registers.L = RL(Registers.L); break; // RL L . 2 8 . Z 0 0 C
                case 0x16: LD8(Registers.HL.Word, RL(LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // RL HL . 2 16 . Z 0 0 C
                case 0x17: Registers.A = RL(Registers.A); break; // RL A . 2 8 . Z 0 0 C
                case 0x18: Registers.B = RR(Registers.B); break; // RR B . 2 8 . Z 0 0 C
                case 0x19: Registers.C = RR(Registers.C); break; // RR C . 2 8 . Z 0 0 C
                case 0x1A: Registers.D = RR(Registers.D); break; // RR D . 2 8 . Z 0 0 C
                case 0x1B: Registers.E = RR(Registers.E); break; // RR E . 2 8 . Z 0 0 C
                case 0x1C: Registers.H = RR(Registers.H); break; // RR H . 2 8 . Z 0 0 C
                case 0x1D: Registers.L = RR(Registers.L); break; // RR L . 2 8 . Z 0 0 C
                case 0x1E: LD8(Registers.HL.Word, RR(LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // RR HL . 2 16 . Z 0 0 C
                case 0x1F: Registers.A = RR(Registers.A); break; // RR A . 2 8 . Z 0 0 C
                case 0x20: Registers.B = SLA(Registers.B); break; // SLA B . 2 8 . Z 0 0 C
                case 0x21: Registers.C = SLA(Registers.C); break; // SLA C . 2 8 . Z 0 0 C
                case 0x22: Registers.D = SLA(Registers.D); break; // SLA D . 2 8 . Z 0 0 C
                case 0x23: Registers.E = SLA(Registers.E); break; // SLA E . 2 8 . Z 0 0 C
                case 0x24: Registers.H = SLA(Registers.H); break; // SLA H . 2 8 . Z 0 0 C
                case 0x25: Registers.L = SLA(Registers.L); break; // SLA L . 2 8 . Z 0 0 C
                case 0x26: LD8(Registers.HL.Word, SLA(LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // SLA HL . 2 16 . Z 0 0 C
                case 0x27: Registers.A = SLA(Registers.A); break; // SLA A . 2 8 . Z 0 0 C
                case 0x28: Registers.B = SRA(Registers.B); break; // SRA B . 2 8 . Z 0 0 C
                case 0x29: Registers.C = SRA(Registers.C); break; // SRA C . 2 8 . Z 0 0 C
                case 0x2A: Registers.D = SRA(Registers.D); break; // SRA D . 2 8 . Z 0 0 C
                case 0x2B: Registers.E = SRA(Registers.E); break; // SRA E . 2 8 . Z 0 0 C
                case 0x2C: Registers.H = SRA(Registers.H); break; // SRA H . 2 8 . Z 0 0 C
                case 0x2D: Registers.L = SRA(Registers.L); break; // SRA L . 2 8 . Z 0 0 C
                case 0x2E: LD8(Registers.HL.Word, SRA(LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // SRA HL . 2 16 . Z 0 0 C
                case 0x2F: Registers.A = SRA(Registers.A); break; // SRA A . 2 8 . Z 0 0 C
                case 0x30: Registers.B = SWAP(Registers.B); break; // SWAP B . 2 8 . Z 0 0 0
                case 0x31: Registers.C = SWAP(Registers.C); break; // SWAP C . 2 8 . Z 0 0 0
                case 0x32: Registers.D = SWAP(Registers.D); break; // SWAP D . 2 8 . Z 0 0 0
                case 0x33: Registers.E = SWAP(Registers.E); break; // SWAP E . 2 8 . Z 0 0 0
                case 0x34: Registers.H = SWAP(Registers.H); break; // SWAP H . 2 8 . Z 0 0 0
                case 0x35: Registers.L = SWAP(Registers.L); break; // SWAP L . 2 8 . Z 0 0 0
                case 0x36: LD8(Registers.HL.Word, SWAP(LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // SWAP HL . 2 16 . Z 0 0 0
                case 0x37: Registers.A = SWAP(Registers.A); break; // SWAP A . 2 8 . Z 0 0 0
                case 0x38: Registers.B = SRL(Registers.B); break; // SRL B . 2 8 . Z 0 0 C
                case 0x39: Registers.C = SRL(Registers.C); break; // SRL C . 2 8 . Z 0 0 C
                case 0x3A: Registers.D = SRL(Registers.D); break; // SRL D . 2 8 . Z 0 0 C
                case 0x3B: Registers.E = SRL(Registers.E); break; // SRL E . 2 8 . Z 0 0 C
                case 0x3C: Registers.H = SRL(Registers.H); break; // SRL H . 2 8 . Z 0 0 C
                case 0x3D: Registers.L = SRL(Registers.L); break; // SRL L . 2 8 . Z 0 0 C
                case 0x3E: LD8(Registers.HL.Word, SRL(LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // SRL HL . 2 16 . Z 0 0 C
                case 0x3F: Registers.A = SRL(Registers.A); break; // SRL A . 2 8 . Z 0 0 C
                case 0x40: BIT(0x01, Registers.B); break; // BIT 0,B . 2 8 . Z 0 1 -
                case 0x41: BIT(0x01, Registers.C); break; // BIT 0,C . 2 8 . Z 0 1 -
                case 0x42: BIT(0x01, Registers.D); break; // BIT 0,D . 2 8 . Z 0 1 -
                case 0x43: BIT(0x01, Registers.E); break; // BIT 0,E . 2 8 . Z 0 1 -
                case 0x44: BIT(0x01, Registers.H); break; // BIT 0,H . 2 8 . Z 0 1 -
                case 0x45: BIT(0x01, Registers.L); break; // BIT 0,L . 2 8 . Z 0 1 -
                case 0x46: BIT(0x01, LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // BIT 0,HL . 2 12 . Z 0 1 -
                case 0x47: BIT(0x01, Registers.A); break; // BIT 0,A . 2 8 . Z 0 1 -
                case 0x48: BIT(0x02, Registers.B); break; // BIT 1,B . 2 8 . Z 0 1 -
                case 0x49: BIT(0x02, Registers.C); break; // BIT 1,C . 2 8 . Z 0 1 -
                case 0x4A: BIT(0x02, Registers.D); break; // BIT 1,D . 2 8 . Z 0 1 -
                case 0x4B: BIT(0x02, Registers.E); break; // BIT 1,E . 2 8 . Z 0 1 -
                case 0x4C: BIT(0x02, Registers.H); break; // BIT 1,H . 2 8 . Z 0 1 -
                case 0x4D: BIT(0x02, Registers.L); break; // BIT 1,L . 2 8 . Z 0 1 -
                case 0x4E: BIT(0x02, LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // BIT 1,HL . 2 12 . Z 0 1 -
                case 0x4F: BIT(0x02, Registers.A); break; // BIT 1,A . 2 8 . Z 0 1 -
                case 0x50: BIT(0x04, Registers.B); break; // BIT 2,B . 2 8 . Z 0 1 -
                case 0x51: BIT(0x04, Registers.C); break; // BIT 2,C . 2 8 . Z 0 1 -
                case 0x52: BIT(0x04, Registers.D); break; // BIT 2,D . 2 8 . Z 0 1 -
                case 0x53: BIT(0x04, Registers.E); break; // BIT 2,E . 2 8 . Z 0 1 -
                case 0x54: BIT(0x04, Registers.H); break; // BIT 2,H . 2 8 . Z 0 1 -
                case 0x55: BIT(0x04, Registers.L); break; // BIT 2,L . 2 8 . Z 0 1 -
                case 0x56: BIT(0x04, LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // BIT 2,HL . 2 12 . Z 0 1 -
                case 0x57: BIT(0x04, Registers.A); break; // BIT 2,A . 2 8 . Z 0 1 -
                case 0x58: BIT(0x08, Registers.B); break; // BIT 3,B . 2 8 . Z 0 1 -
                case 0x59: BIT(0x08, Registers.C); break; // BIT 3,C . 2 8 . Z 0 1 -
                case 0x5A: BIT(0x08, Registers.D); break; // BIT 3,D . 2 8 . Z 0 1 -
                case 0x5B: BIT(0x08, Registers.E); break; // BIT 3,E . 2 8 . Z 0 1 -
                case 0x5C: BIT(0x08, Registers.H); break; // BIT 3,H . 2 8 . Z 0 1 -
                case 0x5D: BIT(0x08, Registers.L); break; // BIT 3,L . 2 8 . Z 0 1 -
                case 0x5E: BIT(0x08, LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // BIT 3,HL . 2 12 . Z 0 1 -
                case 0x5F: BIT(0x08, Registers.A); break; // BIT 3,A . 2 8 . Z 0 1 -
                case 0x60: BIT(0x10, Registers.B); break; // BIT 4,B . 2 8 . Z 0 1 -
                case 0x61: BIT(0x10, Registers.C); break; // BIT 4,C . 2 8 . Z 0 1 -
                case 0x62: BIT(0x10, Registers.D); break; // BIT 4,D . 2 8 . Z 0 1 -
                case 0x63: BIT(0x10, Registers.E); break; // BIT 4,E . 2 8 . Z 0 1 -
                case 0x64: BIT(0x10, Registers.H); break; // BIT 4,H . 2 8 . Z 0 1 -
                case 0x65: BIT(0x10, Registers.L); break; // BIT 4,L . 2 8 . Z 0 1 -
                case 0x66: BIT(0x10, LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // BIT 4,HL . 2 12 . Z 0 1 -
                case 0x67: BIT(0x10, Registers.A); break; // BIT 4,A . 2 8 . Z 0 1 -
                case 0x68: BIT(0x20, Registers.B); break; // BIT 5,B . 2 8 . Z 0 1 -
                case 0x69: BIT(0x20, Registers.C); break; // BIT 5,C . 2 8 . Z 0 1 -
                case 0x6A: BIT(0x20, Registers.D); break; // BIT 5,D . 2 8 . Z 0 1 -
                case 0x6B: BIT(0x20, Registers.E); break; // BIT 5,E . 2 8 . Z 0 1 -
                case 0x6C: BIT(0x20, Registers.H); break; // BIT 5,H . 2 8 . Z 0 1 -
                case 0x6D: BIT(0x20, Registers.L); break; // BIT 5,L . 2 8 . Z 0 1 -
                case 0x6E: BIT(0x20, LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // BIT 5,HL . 2 12 . Z 0 1 -
                case 0x6F: BIT(0x20, Registers.A); break; // BIT 5,A . 2 8 . Z 0 1 -
                case 0x70: BIT(0x40, Registers.B); break; // BIT 6,B . 2 8 . Z 0 1 -
                case 0x71: BIT(0x40, Registers.C); break; // BIT 6,C . 2 8 . Z 0 1 -
                case 0x72: BIT(0x40, Registers.D); break; // BIT 6,D . 2 8 . Z 0 1 -
                case 0x73: BIT(0x40, Registers.E); break; // BIT 6,E . 2 8 . Z 0 1 -
                case 0x74: BIT(0x40, Registers.H); break; // BIT 6,H . 2 8 . Z 0 1 -
                case 0x75: BIT(0x40, Registers.L); break; // BIT 6,L . 2 8 . Z 0 1 -
                case 0x76: BIT(0x40, LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // BIT 6,HL . 2 12 . Z 0 1 -
                case 0x77: BIT(0x40, Registers.A); break; // BIT 6,A . 2 8 . Z 0 1 -
                case 0x78: BIT(0x80, Registers.B); break; // BIT 7,B . 2 8 . Z 0 1 -
                case 0x79: BIT(0x80, Registers.C); break; // BIT 7,C . 2 8 . Z 0 1 -
                case 0x7A: BIT(0x80, Registers.D); break; // BIT 7,D . 2 8 . Z 0 1 -
                case 0x7B: BIT(0x80, Registers.E); break; // BIT 7,E . 2 8 . Z 0 1 -
                case 0x7C: BIT(0x80, Registers.H); break; // BIT 7,H . 2 8 . Z 0 1 -
                case 0x7D: BIT(0x80, Registers.L); break; // BIT 7,L . 2 8 . Z 0 1 -
                case 0x7E: BIT(0x80, LD8(Registers.HL.Word)); _cylcleCounter -= 4; break; // BIT 7,HL . 2 12 . Z 0 1 -
                case 0x7F: BIT(0x80, Registers.A); break; // BIT 7,A . 2 8 . Z 0 1 -
                case 0x80: Registers.B = RES(0x01, Registers.B); break; // RES 0,B . 2 8 . - - - -
                case 0x81: Registers.C = RES(0x01, Registers.C); break; // RES 0,C . 2 8 . - - - -
                case 0x82: Registers.D = RES(0x01, Registers.D); break; // RES 0,D . 2 8 . - - - -
                case 0x83: Registers.E = RES(0x01, Registers.E); break; // RES 0,E . 2 8 . - - - -
                case 0x84: Registers.H = RES(0x01, Registers.H); break; // RES 0,H . 2 8 . - - - -
                case 0x85: Registers.L = RES(0x01, Registers.L); break; // RES 0,L . 2 8 . - - - -
                case 0x86: LD8(Registers.HL.Word, RES(0x01, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // RES 0,HL . 2 16 . - - - -
                case 0x87: Registers.A = RES(0x01, Registers.A); break; // RES 0,A . 2 8 . - - - -
                case 0x88: Registers.B = RES(0x02, Registers.B); break; // RES 1,B . 2 8 . - - - -
                case 0x89: Registers.C = RES(0x02, Registers.C); break; // RES 1,C . 2 8 . - - - -
                case 0x8A: Registers.D = RES(0x02, Registers.D); break; // RES 1,D . 2 8 . - - - -
                case 0x8B: Registers.E = RES(0x02, Registers.E); break; // RES 1,E . 2 8 . - - - -
                case 0x8C: Registers.H = RES(0x02, Registers.H); break; // RES 1,H . 2 8 . - - - -
                case 0x8D: Registers.L = RES(0x02, Registers.L); break; // RES 1,L . 2 8 . - - - -
                case 0x8E: LD8(Registers.HL.Word, RES(0x02, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // RES 1,HL . 2 16 . - - - -
                case 0x8F: Registers.A = RES(0x02, Registers.A); break; // RES 1,A . 2 8 . - - - -
                case 0x90: Registers.B = RES(0x04, Registers.B); break; // RES 2,B . 2 8 . - - - -
                case 0x91: Registers.C = RES(0x04, Registers.C); break; // RES 2,C . 2 8 . - - - -
                case 0x92: Registers.D = RES(0x04, Registers.D); break; // RES 2,D . 2 8 . - - - -
                case 0x93: Registers.E = RES(0x04, Registers.E); break; // RES 2,E . 2 8 . - - - -
                case 0x94: Registers.H = RES(0x04, Registers.H); break; // RES 2,H . 2 8 . - - - -
                case 0x95: Registers.L = RES(0x04, Registers.L); break; // RES 2,L . 2 8 . - - - -
                case 0x96: LD8(Registers.HL.Word, RES(0x04, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // RES 2,HL . 2 16 . - - - -
                case 0x97: Registers.A = RES(0x04, Registers.A); break; // RES 2,A . 2 8 . - - - -
                case 0x98: Registers.B = RES(0x08, Registers.B); break; // RES 3,B . 2 8 . - - - -
                case 0x99: Registers.C = RES(0x08, Registers.C); break; // RES 3,C . 2 8 . - - - -
                case 0x9A: Registers.D = RES(0x08, Registers.D); break; // RES 3,D . 2 8 . - - - -
                case 0x9B: Registers.E = RES(0x08, Registers.E); break; // RES 3,E . 2 8 . - - - -
                case 0x9C: Registers.H = RES(0x08, Registers.H); break; // RES 3,H . 2 8 . - - - -
                case 0x9D: Registers.L = RES(0x08, Registers.L); break; // RES 3,L . 2 8 . - - - -
                case 0x9E: LD8(Registers.HL.Word, RES(0x08, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // RES 3,HL . 2 16 . - - - -
                case 0x9F: Registers.A = RES(0x08, Registers.A); break; // RES 3,A . 2 8 . - - - -
                case 0xA0: Registers.B = RES(0x10, Registers.B); break; // RES 4,B . 2 8 . - - - -
                case 0xA1: Registers.C = RES(0x10, Registers.C); break; // RES 4,C . 2 8 . - - - -
                case 0xA2: Registers.D = RES(0x10, Registers.D); break; // RES 4,D . 2 8 . - - - -
                case 0xA3: Registers.E = RES(0x10, Registers.E); break; // RES 4,E . 2 8 . - - - -
                case 0xA4: Registers.H = RES(0x10, Registers.H); break; // RES 4,H . 2 8 . - - - -
                case 0xA5: Registers.L = RES(0x10, Registers.L); break; // RES 4,L . 2 8 . - - - -
                case 0xA6: LD8(Registers.HL.Word, RES(0x10, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // RES 4,HL . 2 16 . - - - -
                case 0xA7: Registers.A = RES(0x10, Registers.A); break; // RES 4,A . 2 8 . - - - -
                case 0xA8: Registers.B = RES(0x20, Registers.B); break; // RES 5,B . 2 8 . - - - -
                case 0xA9: Registers.C = RES(0x20, Registers.C); break; // RES 5,C . 2 8 . - - - -
                case 0xAA: Registers.D = RES(0x20, Registers.D); break; // RES 5,D . 2 8 . - - - -
                case 0xAB: Registers.E = RES(0x20, Registers.E); break; // RES 5,E . 2 8 . - - - -
                case 0xAC: Registers.H = RES(0x20, Registers.H); break; // RES 5,H . 2 8 . - - - -
                case 0xAD: Registers.L = RES(0x20, Registers.L); break; // RES 5,L . 2 8 . - - - -
                case 0xAE: LD8(Registers.HL.Word, RES(0x20, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // RES 5,HL . 2 16 . - - - -
                case 0xAF: Registers.A = RES(0x20, Registers.A); break; // RES 5,A . 2 8 . - - - -
                case 0xB0: Registers.B = RES(0x40, Registers.B); break; // RES 6,B . 2 8 . - - - -
                case 0xB1: Registers.C = RES(0x40, Registers.C); break; // RES 6,C . 2 8 . - - - -
                case 0xB2: Registers.D = RES(0x40, Registers.D); break; // RES 6,D . 2 8 . - - - -
                case 0xB3: Registers.E = RES(0x40, Registers.E); break; // RES 6,E . 2 8 . - - - -
                case 0xB4: Registers.H = RES(0x40, Registers.H); break; // RES 6,H . 2 8 . - - - -
                case 0xB5: Registers.L = RES(0x40, Registers.L); break; // RES 6,L . 2 8 . - - - -
                case 0xB6: LD8(Registers.HL.Word, RES(0x40, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // RES 6,HL . 2 16 . - - - -
                case 0xB7: Registers.A = RES(0x40, Registers.A); break; // RES 6,A . 2 8 . - - - -
                case 0xB8: Registers.B = RES(0x80, Registers.B); break; // RES 7,B . 2 8 . - - - -
                case 0xB9: Registers.C = RES(0x80, Registers.C); break; // RES 7,C . 2 8 . - - - -
                case 0xBA: Registers.D = RES(0x80, Registers.D); break; // RES 7,D . 2 8 . - - - -
                case 0xBB: Registers.E = RES(0x80, Registers.E); break; // RES 7,E . 2 8 . - - - -
                case 0xBC: Registers.H = RES(0x80, Registers.H); break; // RES 7,H . 2 8 . - - - -
                case 0xBD: Registers.L = RES(0x80, Registers.L); break; // RES 7,L . 2 8 . - - - -
                case 0xBE: LD8(Registers.HL.Word, RES(0x80, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // RES 7,HL . 2 16 . - - - -
                case 0xBF: Registers.A = RES(0x80, Registers.A); break; // RES 7,A . 2 8 . - - - -
                case 0xC0: Registers.B = SET(0x01, Registers.B); break; // SET 0,B . 2 8 . - - - -
                case 0xC1: Registers.C = SET(0x01, Registers.C); break; // SET 0,C . 2 8 . - - - -
                case 0xC2: Registers.D = SET(0x01, Registers.D); break; // SET 0,D . 2 8 . - - - -
                case 0xC3: Registers.E = SET(0x01, Registers.E); break; // SET 0,E . 2 8 . - - - -
                case 0xC4: Registers.H = SET(0x01, Registers.H); break; // SET 0,H . 2 8 . - - - -
                case 0xC5: Registers.L = SET(0x01, Registers.L); break; // SET 0,L . 2 8 . - - - -
                case 0xC6: LD8(Registers.HL.Word, SET(0x01, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // SET 0,HL . 2 16 . - - - -
                case 0xC7: Registers.A = SET(0x01, Registers.A); break; // SET 0,A . 2 8 . - - - -
                case 0xC8: Registers.B = SET(0x02, Registers.B); break; // SET 1,B . 2 8 . - - - -
                case 0xC9: Registers.C = SET(0x02, Registers.C); break; // SET 1,C . 2 8 . - - - -
                case 0xCA: Registers.D = SET(0x02, Registers.D); break; // SET 1,D . 2 8 . - - - -
                case 0xCB: Registers.E = SET(0x02, Registers.E); break; // SET 1,E . 2 8 . - - - -
                case 0xCC: Registers.H = SET(0x02, Registers.H); break; // SET 1,H . 2 8 . - - - -
                case 0xCD: Registers.L = SET(0x02, Registers.L); break; // SET 1,L . 2 8 . - - - -
                case 0xCE: LD8(Registers.HL.Word, SET(0x02, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // SET 1,HL . 2 16 . - - - -
                case 0xCF: Registers.A = SET(0x02, Registers.A); break; // SET 1,A . 2 8 . - - - -
                case 0xD0: Registers.B = SET(0x04, Registers.B); break; // SET 2,B . 2 8 . - - - -
                case 0xD1: Registers.C = SET(0x04, Registers.C); break; // SET 2,C . 2 8 . - - - -
                case 0xD2: Registers.D = SET(0x04, Registers.D); break; // SET 2,D . 2 8 . - - - -
                case 0xD3: Registers.E = SET(0x04, Registers.E); break; // SET 2,E . 2 8 . - - - -
                case 0xD4: Registers.H = SET(0x04, Registers.H); break; // SET 2,H . 2 8 . - - - -
                case 0xD5: Registers.L = SET(0x04, Registers.L); break; // SET 2,L . 2 8 . - - - -
                case 0xD6: LD8(Registers.HL.Word, SET(0x04, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // SET 2,HL . 2 16 . - - - -
                case 0xD7: Registers.A = SET(0x04, Registers.A); break; // SET 2,A . 2 8 . - - - -
                case 0xD8: Registers.B = SET(0x08, Registers.B); break; // SET 3,B . 2 8 . - - - -
                case 0xD9: Registers.C = SET(0x08, Registers.C); break; // SET 3,C . 2 8 . - - - -
                case 0xDA: Registers.D = SET(0x08, Registers.D); break; // SET 3,D . 2 8 . - - - -
                case 0xDB: Registers.E = SET(0x08, Registers.E); break; // SET 3,E . 2 8 . - - - -
                case 0xDC: Registers.H = SET(0x08, Registers.H); break; // SET 3,H . 2 8 . - - - -
                case 0xDD: Registers.L = SET(0x08, Registers.L); break; // SET 3,L . 2 8 . - - - -
                case 0xDE: LD8(Registers.HL.Word, SET(0x08, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // SET 3,HL . 2 16 . - - - -
                case 0xDF: Registers.A = SET(0x08, Registers.A); break; // SET 3,A . 2 8 . - - - -
                case 0xE0: Registers.B = SET(0x10, Registers.B); break; // SET 4,B . 2 8 . - - - -
                case 0xE1: Registers.C = SET(0x10, Registers.C); break; // SET 4,C . 2 8 . - - - -
                case 0xE2: Registers.D = SET(0x10, Registers.D); break; // SET 4,D . 2 8 . - - - -
                case 0xE3: Registers.E = SET(0x10, Registers.E); break; // SET 4,E . 2 8 . - - - -
                case 0xE4: Registers.H = SET(0x10, Registers.H); break; // SET 4,H . 2 8 . - - - -
                case 0xE5: Registers.L = SET(0x10, Registers.L); break; // SET 4,L . 2 8 . - - - -
                case 0xE6: LD8(Registers.HL.Word, SET(0x10, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // SET 4,HL . 2 16 . - - - -
                case 0xE7: Registers.A = SET(0x10, Registers.A); break; // SET 4,A . 2 8 . - - - -
                case 0xE8: Registers.B = SET(0x20, Registers.B); break; // SET 5,B . 2 8 . - - - -
                case 0xE9: Registers.C = SET(0x20, Registers.C); break; // SET 5,C . 2 8 . - - - -
                case 0xEA: Registers.D = SET(0x20, Registers.D); break; // SET 5,D . 2 8 . - - - -
                case 0xEB: Registers.E = SET(0x20, Registers.E); break; // SET 5,E . 2 8 . - - - -
                case 0xEC: Registers.H = SET(0x20, Registers.H); break; // SET 5,H . 2 8 . - - - -
                case 0xED: Registers.L = SET(0x20, Registers.L); break; // SET 5,L . 2 8 . - - - -
                case 0xEE: LD8(Registers.HL.Word, SET(0x20, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // SET 5,HL . 2 16 . - - - -
                case 0xEF: Registers.A = SET(0x20, Registers.A); break; // SET 5,A . 2 8 . - - - -
                case 0xF0: Registers.B = SET(0x40, Registers.B); break; // SET 6,B . 2 8 . - - - -
                case 0xF1: Registers.C = SET(0x40, Registers.C); break; // SET 6,C . 2 8 . - - - -
                case 0xF2: Registers.D = SET(0x40, Registers.D); break; // SET 6,D . 2 8 . - - - -
                case 0xF3: Registers.E = SET(0x40, Registers.E); break; // SET 6,E . 2 8 . - - - -
                case 0xF4: Registers.H = SET(0x40, Registers.H); break; // SET 6,H . 2 8 . - - - -
                case 0xF5: Registers.L = SET(0x40, Registers.L); break; // SET 6,L . 2 8 . - - - -
                case 0xF6: LD8(Registers.HL.Word, SET(0x40, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // SET 6,HL . 2 16 . - - - -
                case 0xF7: Registers.A = SET(0x40, Registers.A); break; // SET 6,A . 2 8 . - - - -
                case 0xF8: Registers.B = SET(0x80, Registers.B); break; // SET 7,B . 2 8 . - - - -
                case 0xF9: Registers.C = SET(0x80, Registers.C); break; // SET 7,C . 2 8 . - - - -
                case 0xFA: Registers.D = SET(0x80, Registers.D); break; // SET 7,D . 2 8 . - - - -
                case 0xFB: Registers.E = SET(0x80, Registers.E); break; // SET 7,E . 2 8 . - - - -
                case 0xFC: Registers.H = SET(0x80, Registers.H); break; // SET 7,H . 2 8 . - - - -
                case 0xFD: Registers.L = SET(0x80, Registers.L); break; // SET 7,L . 2 8 . - - - -
                case 0xFE: LD8(Registers.HL.Word, SET(0x80, LD8(Registers.HL.Word))); _cylcleCounter -= 8; break; // SET 7,HL . 2 16 . - - - -
                case 0xFF: Registers.A = SET(0x80, Registers.A); break; // SET 7,A . 2 8 . - - - -
                //default: Console.WriteLine($"PC {Registers.PC.Word} OPCode {opCode.ToString("X2")} not implemented"); break;
            }

        }
        #endregion

        public virtual void EI()
        {
            _cylcleCounter += 8;
            IME = true;
        }
        public virtual void HALT()
        {
            // TODO: Implement
            if (!IME)
            {
                if ((IE & IF) != 0)
                {
                    HALTED = true;
                    Registers.PC--;
                }
                else
                {
                    HALT_BUG = true;
                }
            }
            _cylcleCounter += 4;
        }

        #region Logic Operations
        public virtual void OR(byte v2)
        {
            Registers.A = (byte)(Registers.A | v2);
            UpdateZeroFlag(Registers.A);
            Registers.SubstractFlag = false;
            Registers.HaltFlag = false;
            Registers.CarryFlag = false;
            _cylcleCounter += 4;
        }
        public virtual void XOR( byte v2)
        {
            Registers.A = (byte)(Registers.A ^ v2);
            UpdateZeroFlag(Registers.A);
            Registers.SubstractFlag = false;
            Registers.HaltFlag = false;
            Registers.CarryFlag = false;
            _cylcleCounter += 4;
        }
        public virtual void AND(byte v2)
        {
            Registers.A = (byte)(Registers.A & v2);
            UpdateZeroFlag(Registers.A);
            Registers.SubstractFlag = false;
            Registers.HaltFlag = true;
            Registers.CarryFlag = false;
            _cylcleCounter += 4;
        }
        public virtual void CP(byte v1, byte v2)
        {
            var result = v1 - v2;
            UpdateZeroFlag(result);
            Registers.SubstractFlag = true;
            UpdateHaltFlagSub(v1, v2);
            UpdateCarryFlag(result);
            _cylcleCounter += 4;
        }
        public virtual byte RLC(byte v)
        {
            byte result = (byte)((v << 1) | (v >> 7));
            UpdateZeroFlag(result);
            Registers.SubstractFlag = false;
            Registers.HaltFlag = false;
            UpdateCarryFlagMost(v);
            _cylcleCounter += 8;
            return result;
        }
        public virtual byte RRC(byte v)
        {
            var result = ((v >> 1) | (v << 7));
            UpdateZeroFlag(result);
            Registers.SubstractFlag = false;
            Registers.HaltFlag = false;
            UpdateCarryFlagLeast(v);
            _cylcleCounter += 8;
            return (byte)result;
        }
        public virtual byte RL(byte v)
        {
            var prevC = Registers.CarryFlag;
            var result = ((v << 1) | (prevC ? 1 : 0));
            UpdateZeroFlag(result);
            Registers.SubstractFlag = false;
            Registers.HaltFlag = false;
            UpdateCarryFlagMost(v);
            _cylcleCounter += 8;
            return (byte)result;
        }
        public virtual byte RR(byte v)
        {
            var prevC = Registers.CarryFlag;
            var result = ((v >> 1) | (prevC ? 0x80 : 0));
            UpdateZeroFlag(result);
            Registers.SubstractFlag = false;
            Registers.HaltFlag = false;
            UpdateCarryFlagLeast(v);
            _cylcleCounter += 8;
            return (byte)result;
        }
        public virtual byte SLA(byte v)
        {
            var result = (v << 1);
            UpdateZeroFlag(result);
            Registers.SubstractFlag = false;
            Registers.HaltFlag = false;
            UpdateCarryFlagMost(v);
            _cylcleCounter += 8;
            return (byte)result;
        }
        public virtual byte SRA(byte v)
        {
            var result = (v >> 1 | (v & 0x80));
            UpdateZeroFlag(result);
            Registers.SubstractFlag = false;
            Registers.HaltFlag = false;
            Registers.CarryFlag = (v & 0x1) != 0;
            _cylcleCounter += 8;
            return (byte)result;
        }
        public virtual byte SWAP(byte v)
        {
            var result = (byte)((v & 0xF0) >> 4 | (v & 0x0F) << 4);
            UpdateZeroFlag(result);
            Registers.SubstractFlag = false;
            Registers.HaltFlag = false;
            Registers.CarryFlag = false;
            _cylcleCounter += 8;
            return result;
        }
        public virtual byte SRL(byte v)
        {
            var result = (v >> 1);
            UpdateZeroFlag(result);
            Registers.SubstractFlag = false;
            Registers.HaltFlag = false;
            UpdateCarryFlagLeast(v);
            _cylcleCounter += 8;
            return (byte)result;
        }
        public virtual void BIT(byte mask, byte v)
        {
            UpdateZeroFlag(v & mask);
            Registers.SubstractFlag = false;
            Registers.HaltFlag = true; 
            _cylcleCounter += 8;
        }
        public virtual byte RES(byte mask, byte v)
        {
            _cylcleCounter += 8;
            return (byte)(v & ~mask);
        }
        public virtual byte SET(byte mask, byte v)
        {
            _cylcleCounter += 8;
            return (byte)(v | mask);
        }

        #endregion

        #region Move commands

        /// <summary>
        /// Reads 8bits from memory (byte)
        /// </summary>
        /// <param name="address"></param>
        public virtual byte LDP8(ushort address)
        {
            return Ram.Read(Ram.Read(address));
        }
        public virtual void LDP8(byte value)
        {
            Ram.Write(LD16(), value);
        }
        public virtual byte LD8(ushort address) {  _cylcleCounter += 8; return Ram.Read(address); }
        public virtual void LD8(ushort address, byte value) { Ram.Write(address, value); _cylcleCounter += 8; }
        public virtual byte LDHP8(ushort address) { _cylcleCounter -= 4; return LD8((ushort)(0xFF00 + LD8(address))); }
        public virtual void LDH8(ushort address, byte value) => LD8((ushort)(0xFF00 + address), value);
        public virtual void LDHA8(ushort address, byte value) { LD8((ushort)(0xFF00 + LD8(address)), value); _cylcleCounter -= 4; }
        public virtual byte LDHA8(ushort address) => LD8((ushort)(0xFF00 + address));
        public virtual ushort LD16()
        {
            var r = new Register();
            r.Low = LD8(Registers.PC.Word);
            Registers.PC++;
            r.High = LD8(Registers.PC.Word);
            Registers.PC++;
            _cylcleCounter -= 4;
            return r.Word;
        }
        public virtual byte LDP16()
        {
            _cylcleCounter += 4;
            return Ram.Read(LD16());
        }
        public virtual void LD16(ushort value)
        {
            var r = new Register(value);
            LD8(Registers.PC.Word, r.Low);
            Registers.PC++;
            LD8(Registers.PC.Word, r.High);
            Registers.PC++;
            _cylcleCounter -= 4;
        }
        public virtual void LDP16(ushort value)
        {
            var r = new Register(value);
            var address = new Register(LD16());
            LD8(address.Word, r.Low);
            address++;
            LD8(address.Word, r.High);
            _cylcleCounter -= 8;
        }
        #endregion

        #region Addings

        public virtual ushort ADDr8(ushort v1)
        {
            var v2 = LD16();
            Registers.PC--;
            Registers.ZeroFlag = false;
            Registers.SubstractFlag = false;
            Registers.HaltFlag = ((v1 & 0x0F) + (v2 & 0x0F)) > 0x0F;
            UpdateCarryFlag((byte)v1 + v2);
            return (ushort)(v1 + (sbyte)v2);
        }

        public virtual void ADC(byte v2)
        {
            var v1 = Registers.A;
            var carry = Registers.CarryFlag ? 1 : 0;
            var result = v1 + v2 + carry;
            UpdateZeroFlag(result);
            Registers.SubstractFlag = false;
            if (Registers.CarryFlag)
                Registers.HaltFlag = ((v1 & 0x0F) + (v2 & 0x0F)) >= 0x0F;
            else
                Registers.HaltFlag = ((v1 & 0x0F) + (v2 & 0x0F)) > 0x0F;
            UpdateCarryFlag(result);
            Registers.A = (byte)result;
            _cylcleCounter += 4;
        }

        public virtual void ADD( byte v2)
        {
            var v1 = Registers.A;
            var result = (ushort)(v1 + v2);
            UpdateZeroFlag(result);
            Registers.SubstractFlag = false;
            Registers.HaltFlag = ((v1 & 0x0F) + (v2 & 0x0F)) > 0x0F;
            Registers.CarryFlag = ((byte)result) < result;
            Registers.A =(byte)result;
            _cylcleCounter += 4;
        }

        public virtual ushort ADD(ushort v1, ushort v2)
        {
            var sumValue = (v1 + v2);
            Registers.SubstractFlag = false;
            Registers.HaltFlag = ((v1 & 0xFFF) + (v2 & 0xFFF)) > 0xFFF;
            Registers.CarryFlag = sumValue > 0xFFFF;
            _cylcleCounter += 8;
            return (ushort)sumValue;
        }
        #endregion

        #region Substractions
        public virtual byte SBC8(byte v1, byte v2)
        {
            var value = (v1 - v2 - (Registers.CarryFlag ? 1 : 0));

            UpdateZeroFlag(value);
            Registers.SubstractFlag = true;
            UpdateHaltFlagCarry(v1, (ushort)v2);
            if (Registers.CarryFlag)
                UpdateHaltFlagSubCarry(v1, v2);
            else
                UpdateHaltFlagSub(v1, v2);
            UpdateCarryFlag(value);
            return (byte)value;
        }
        #endregion

        #region Decrement
        public virtual void SUBC(byte v2)
        {
            var v1 = Registers.A;
            var carry = Registers.CarryFlag ? 1 : 0;
            var result = v1 - v2 - carry;
            UpdateZeroFlag((byte)result);
            Registers.SubstractFlag = true;
            if (Registers.CarryFlag)
                UpdateHaltFlagSubCarry(v1, v2);
            else
                UpdateHaltFlagSub(v1, v2);
            UpdateCarryFlag(result);
            Registers.A = (byte)result;
            _cylcleCounter += 4;
        }
        public virtual void SUB(byte v2)
        {
            var v1 = Registers.A;
            var result = v1 - v2;
            UpdateZeroFlag(result);
            Registers.SubstractFlag = true;
            UpdateHaltFlagSub(v1, v2);
            UpdateCarryFlag(result);
            Registers.A = (byte)result;
            _cylcleCounter += 4;
        }
        public virtual byte DEC(byte value, byte decrement)
        {
            UpdateHaltFlagSub(value, decrement);
            value -= decrement;
            UpdateZeroFlag(value);
            Registers.SubstractFlag = true;
            _cylcleCounter += 4;
            return value;
        }

        public virtual void DECA(ushort address, byte increment)
        {
            var value = LD8(address);
            LD8(address, DEC(value, increment));
            _cylcleCounter -= 4;
        }
        #endregion

        #region Increments
        public virtual void INCA(ushort address, byte increment)
        {
            var value = LD8(address);
            LD8(address, INC(value, increment));
            _cylcleCounter -= 4;
        }

        public virtual byte INC(byte value, byte increment)
        {
            Registers.HaltFlag = ((value & 0xF) + (increment & 0xF)) > 0xF;
            value += increment;
            UpdateZeroFlag(value);
            Registers.SubstractFlag = false;
            _cylcleCounter += 4;
            return value;
        }
        public virtual ushort INC(ushort value, byte increment)
        {
            value += increment;
            _cylcleCounter += 4;
            return value;
        }
        #endregion

        #region Jumps
        public virtual void RET(bool flag = false)
        {
            _cylcleCounter += 8;
            if (flag)
            {
                Registers.PC.Word = POP();
            }
        }
        /// <summary>
        /// Jumps backwards or fo
        /// </summary>
        /// <param name="flag"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void JR(bool flag)
        {
            _cylcleCounter += 8;
            if (flag)
            {
                Registers.PC += (sbyte)LD8(Registers.PC.Word);
                _cylcleCounter -= 4;
            }
            Registers.PC++;
        }
        /// <summary>
        /// Jump to address
        /// </summary>
        /// <param name="address"></param>
        public virtual void JP(bool flag)
        {
            _cylcleCounter += 12;
            if (flag)
            {
                Registers.PC.Word = LD16();
            }
            else
            {
                Registers.PC += 2;
            }
        }

        #endregion

        #region Others
        public virtual ushort POP()
        {
            var low = LD8(Registers.SP);
            Registers.SP++;
            var high = LD8(Registers.SP);
            Registers.SP++;
            _cylcleCounter -= 4;
            return (ushort)((high << 8) | low);
        }
        public virtual void CCF()
        {
            Registers.CarryFlag = !Registers.CarryFlag;
            Registers.HaltFlag = false;
            Registers.SubstractFlag = false;
            _cylcleCounter += 4;
        }
        public virtual void SCF()
        {
            Registers.CarryFlag = true;
            Registers.HaltFlag = false;
            Registers.SubstractFlag = false;
            _cylcleCounter += 4;
        }
        public virtual void DAA()
        {
            var a = Registers.A;
            var cf = Registers.CarryFlag;
            var hf = Registers.HaltFlag;
            var sf = Registers.SubstractFlag;

            if (sf)
            { // sub
                if (cf) { a -= 0x60; }
                if (hf) { a -= 0x6; }
            }
            else
            { // add
                if (cf || (a > 0x99)) { a += 0x60; Registers.CarryFlag = true; }
                if (hf || (a & 0xF) > 0x9) { a += 0x6; }
            }
            Registers.A = a;

            UpdateZeroFlag(Registers.A);
            Registers.HaltFlag = false;
            //Registers.HaltFlag = hfAfter;
            _cylcleCounter += 4;
        }

        byte AdjustBCD(byte a)
        {
            // Ajuste de los dígitos bajos (0-9)
            var flagC = Registers.CarryFlag;
            var flagH = Registers.HaltFlag;

            byte digitosBajos = (byte)(a & 0x0F);
            byte digitosAltos = (byte)((a >> 4) & 0x0F);

            if ((digitosBajos > 9) || flagH)
            {
                a += 6;
                flagH = ((digitosBajos + 6) & 0x10) == 0x10;
            }

            if ((digitosAltos > 9) || flagC)
            {
                a += 0x60;
                Registers.CarryFlag = ((a + 0x60) & 0x100) == 0x100;
            }
            return a;
        }
        public virtual void DAA2()
        {
            var a = Registers.A;
            var lowNibble = (byte)(Registers.A & 0x0F);
            var highNibble = (byte)((Registers.A & 0xF0) >> 4);
            var cf = Registers.CarryFlag;
            var hf = Registers.HaltFlag;
            var nf = Registers.ZeroFlag;
            var sf = Registers.SubstractFlag;
            int adjustment = (nf,sf,hf,cf,lowNibble,highNibble) switch
            {
                (false, false, false, false, <= 9,> 9) => 0x60,
                (false, false, false, true,  <= 9, > 9) => 0x60,
                (false, false, false, true,  <= 9, <= 9) => 0x60,
                (false, false, true,  false, > 9, > 9) => 0x66,
                (false, false, true,  false, <= 9, <= 9) => 0x06,
                (false, false, true,  false, > 9, <= 9) => 0x66,
                //(false, false, true,  false, > 9, <= 9) => 0x06,
                (false, false, true,  true,  > 9, > 9) => 0x66,
                (false, false, true,  true,  > 9, <= 9) => 0x66,
                (false, false, true,  true,  <= 9, <= 9) => 0x66,
                (false, true,  true,  true,  <= 9, > 9) => -0x66,
                (false, true,  true,  false, > 9, <= 9) => -0x06,
                (false, true,  true,  true,  > 9, <= 9) => -0x66,
                (false, true,  true,  false, <= 9, > 9) => -0x06,
                (false, true,  true,  false, > 9, > 9) => -0x06,
                (false, true,  false, true,  <= 9, <= 9) => -0x60,
                (false, true,  false, false, <= 9, <= 9) => 0,
                (false, true,  false, false, <= 9, > 9) => 0,
                (true,  false, false, false, <= 9, > 9) => 0x60,
                (true,  false, false, true,  <= 9, > 9) => 0x60,
                (true,  false, false, true,  > 9, <= 9) => 0x66,
                (true,  true,  false, true,  > 9, <= 9) => -0x60,
                (true,  false, true,  false, <= 9, <= 9) => 0x06,
                (true,  false, true,  false, > 9, > 9) => 0x66,
                (true,  false, true,  true,  > 9, <= 9) => 0x66,
                (true,  true,  false, true,  <= 9, <= 9) => -0x60,
                (true,  false, true,  false, > 9, <= 9) => 0x06,
                (true,  true,  true,  false, <= 9, <= 9) => -0x06,
                (true,  true,  true,  false, > 9, <= 9) => -0x06,
                (true,  true,  true,  false, > 9, > 9) => -0x06,
                (true,  true,  true,  true,  <= 9, > 9) => -0x66,
                (true,  true,  true,  true,  <= 9, <= 9) => -0x66,
                _ => 0

            };

            Registers.CarryFlag = (nf, sf, hf, cf, lowNibble, highNibble) switch
            {
                (false, false, true, false, > 9, <= 9) => true,
                (true, true, false, true, <= 9, <= 9) => true,
                (false, false, true, true, <= 9, <= 9) => true,
                (false, false, false, false, <= 9, > 9) => true,
                (true, false, false, false, <= 9, > 9) => true,
                (false, false, true, true, > 9, <= 9) => true,
                (false, true, false, true, <= 9, <= 9) => true,
                (true, false, false, true, <= 9, > 9) => true,
                (false, false, false, true, <= 9, > 9) => true,
                (true, false, true, false, > 9, > 9) => true,
                (true, true, true, true, <= 9, <= 9) => true,
                (false, false, true, true, > 9, > 9) => true,
                (true, false, false, true, > 9, <= 9) => true,
                (true, true, false, true, > 9, <= 9) => true,
                (true, true, true, false, > 9, > 9) => true,
                (false, true, true, true, > 9, <= 9) => true,
                (false, false, false, true, <= 9, <= 9) => true,
                (false, false, true, false, > 9, > 9) => true,
                (true, true, true, true, <= 9, > 9) => true,
                (false, true, true, true, <= 9, > 9) => true,
                _ => false

            };
            Registers.A = (byte)(Registers.A + adjustment);
            UpdateZeroFlag(Registers.A);
            Registers.HaltFlag = false;
            //Registers.HaltFlag = hfAfter;
            _cylcleCounter += 4;
        }
        public virtual void STOP() { }
        public virtual byte RRA(byte value)
        {
            Registers.ZeroFlag = false;
            Registers.SubstractFlag = false;
            Registers.HaltFlag = false;
            var prevFlag = Registers.CarryFlag;
            UpdateCarryFlagLeast(value);
            _cylcleCounter += 4;
            return (byte)((value >> 1) | (prevFlag ? 0x80 : 0));
        }

        public virtual byte RRCA(byte value)
        {
            Registers.ZeroFlag = false;
            Registers.SubstractFlag = false;
            Registers.HaltFlag = false;
            UpdateCarryFlagLeast(value); 
            _cylcleCounter += 8;
            return (byte)((value >> 1) | (value << 7));
        }
        public virtual byte RLCA(byte value)
        {
            Registers.ZeroFlag = false;
            Registers.SubstractFlag = false;
            Registers.HaltFlag = false;
            UpdateCarryFlagMost(value);
            _cylcleCounter += 4;
            return (byte)((value << 1) | (value >> 7));
        }
        public virtual byte RLA(byte value)
        {
            Registers.ZeroFlag = false;
            Registers.SubstractFlag = false;
            Registers.HaltFlag = false;
            var oldFlag = Registers.CarryFlag;
            UpdateCarryFlagMost(value);
            _cylcleCounter += 4;
            return (byte)((value << 1) | (oldFlag ? 1 : 0));
        }
        public virtual void CALL(bool flag)
        {
            if (flag)
            {
                CALL();
            }
            else
            {
                Registers.PC += 2;
            }
        }

        public virtual void CALL()
        {
            var low = LD8(Registers.PC.Word);
            Registers.PC++;
            var high = LD8(Registers.PC.Word);
            Registers.PC++;
            PUSH(Registers.PC.High, Registers.PC.Low);
            Registers.PC.Word = (ushort)((high << 8) | low);
            _cylcleCounter -= 8;
        }
        public virtual void CPL()
        {
            Registers.A = (byte)~Registers.A;
            Registers.HaltFlag = true;
            Registers.SubstractFlag = true;
            _cylcleCounter += 4;
        }
        public virtual void DI()
        {
            Registers.IME = false;
            _cylcleCounter += 4;

        }

        public virtual void RST(byte OPCode)
        {
            PUSH(Registers.PC.High, Registers.PC.Low);
            Registers.PC.Word = OPCode;
        }
        #endregion

        #region Memory and stack manipulation
        public virtual void PUSH(byte high, byte low)
        {
            Registers.SP--;
            LD8(Registers.SP, high);
            Registers.SP--;
            LD8(Registers.SP, low);
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
        public virtual void UpdateHaltFlagCarry(ushort w1, ushort w2) => Registers.HaltFlag = ((w1 & 0xFFF) + (w2 & 0xFFF)) > 0xFFF;
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
            var pc = Ram.Read((ushort)(Registers.PC.Word)).ToString("X2");
            var pc1 = Ram.Read((ushort)(Registers.PC.Word + 1)).ToString("X2");
            var pc2 = Ram.Read((ushort)(Registers.PC.Word + 2)).ToString("X2");
            var pc3 = Ram.Read((ushort)(Registers.PC.Word + 3)).ToString("X2");
            return $"A: {Registers.A.ToString("X2")} F: {Registers.F.ToString("X2")} B: {Registers.B.ToString("X2")} C: {Registers.C.ToString("X2")} D: {Registers.D.ToString("X2")} E: {Registers.E.ToString("X2")} H: {Registers.H.ToString("X2")} L: {Registers.L.ToString("X2")} SP: {Registers.SP.ToString("X2")} PC: {Registers.PC.Word.ToString("X4")} ({pc} {pc1} {pc2} {pc3})";
        }

        internal void Initialize()
        {
            Registers.A = 0x11;
            Registers.ZeroFlag = true;
            Registers.BC.Word = 0;
            Registers.DE.Word = 0;
            Registers.HL.Word = 0;
            Registers.PC.Word = 0x0100;
            Registers.SP = 0xFFFE;
            IF = 0xE1;
            IE = 0;
        }
        #endregion
    }
}