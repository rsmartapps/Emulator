using Emulator.Domain;

namespace Emulator.GBC;

public class GBCMachine : IMachine
{
    GBCPPU PPU { get { return (GBCPPU)Hardware.PPU; } }
    public GBCMachine()
    {
        Hardware.Memory = new GBCMemory();
        Hardware.CPU = new GBCCPU();
        Hardware.PPU = new GBCPPU();
    }

    public void ExecuteGame()
    {
        while(true)
        {
            Hardware.CPU.Execute();
        }
    }

    public Task LoadGame(string path)
    {
        var file = File.ReadAllBytes(path);
        Hardware.Memory.Load(file);


        return Task.CompletedTask;
    }
}

