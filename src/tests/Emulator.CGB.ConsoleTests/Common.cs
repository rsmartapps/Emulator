using Emulator.CGB.CPU;

namespace Emulator.CGB.ConsoleTests;

public static class Common
{
    internal static void InitializeCPU(CGBCPU gbcpu, Initial initial)
    {
        gbcpu.Registers.A = initial.a;
        gbcpu.Registers.F = initial.f;
        gbcpu.Registers.B = initial.b;
        gbcpu.Registers.C = initial.c;
        gbcpu.Registers.D = initial.d;
        gbcpu.Registers.E = initial.e;
        gbcpu.Registers.H = initial.h;
        gbcpu.Registers.L = initial.l;
        gbcpu.Registers.PC.Word = initial.pc;
        gbcpu.Registers.SP = initial.sp;
        gbcpu.IME = initial.ime == 0 ? false : true;
        gbcpu.IE = (byte)initial.ie;

        for (var row = 0; row < initial.ram.Length; row++)
        {
            gbcpu.Ram.Write((ushort)initial.ram[row][0], (byte)initial.ram[row][1]);
        }
    }
    internal static Final GetCPUAsFinal(CGBCPU gbcpu)
    {
        return new Final
        {
            a = gbcpu.Registers.A,
            b = gbcpu.Registers.B,
            c = gbcpu.Registers.C,
            d = gbcpu.Registers.D,
            e = gbcpu.Registers.E,
            f = gbcpu.Registers.F,
            h = gbcpu.Registers.H,
            l = gbcpu.Registers.L,
            pc = gbcpu.Registers.PC.Word,
            sp = gbcpu.Registers.SP,
            ime = gbcpu.IME ? 1 : 0,
            ram = gbcpu.Ram.RAM.Select(s => new int[] { s.Key,s.Value}).ToArray(),
        };
    }
}
