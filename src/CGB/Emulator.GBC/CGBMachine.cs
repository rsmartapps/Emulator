using Emulator.CGB.CPU;
using Emulator.CGB.PPU;
using Emulator.CGB.Memory;
using Emulator.Domain;

namespace Emulator.GBC;

internal class CGBMachine : IMachine
{
    public CGBCPU CPU { get; set; }
    public PPUUnit PPU { get; set; }
    public CGBMemoryBus Memory { get; set; }

    public Task InsertCartRidge(string path)
    {
        throw new NotImplementedException();
    }

    public void RunGame()
    {
        throw new NotImplementedException();
    }
}
