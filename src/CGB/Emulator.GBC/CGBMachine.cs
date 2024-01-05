using Emulator.CGB.CPU;
using Emulator.CGB.PPU;
using Emulator.CGB.Memory;
using Emulator.Domain;
using Emulator.CGB.Memory.MBC;

namespace Emulator.GBC;

internal class CGBMachine : IMachine
{
    public CGBCPU CPU { get; set; }
    public PPUUnit PPU { get; set; }
    public CGBMemoryBus Memory { get; set; }
    private CancellationTokenSource _cancellationTokenSource;
    private Thread thread;

    public CGBMachine()
    {
        Memory = new CGBMemoryBus();
        PPU = new PPUUnit(Memory);
        CPU = new CGBCPU(Memory);
    }

    public void InsertCartRidge(string path)
    {
        var ROM = File.ReadAllBytes(path);
        Memory.InsertCartridge(ROM);
        CPU.Initialize();
    }

    public void RunGame()
    {
        if(_cancellationTokenSource is not null)
            _cancellationTokenSource.Cancel();

        _cancellationTokenSource = new CancellationTokenSource();
        //thread = new Thread(new ThreadStart(() => {
        //    while (!_cancellationTokenSource.IsCancellationRequested)
        //    {
        //        ExecuteGame();
        //    }
        //}));
        //Task.Run(() => { 
        //    while(!_cancellationTokenSource.IsCancellationRequested)
        //    {
        //        ExecuteGame();
        //    }
        //}, _cancellationTokenSource.Token);
        ExecuteGame();
    }

    private void ExecuteGame()
    {
        CPU.ProcessOperation();
        PPU.Update(4);
        CPU.ProcessInterrupts();
    }

    public void StopGame()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = null;
    }
}
