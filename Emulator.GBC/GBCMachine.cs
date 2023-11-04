using Emulator.Domain;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.GBC
{
    public class GBCMachine : IMachine
    {

        public GBCMachine()
        {
            Hardware.CPU = new GBCCPU();
            Hardware.Memory = new GBCMemory();
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
}
