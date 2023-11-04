using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.Domain
{
    public interface IMachine
    {
        Task LoadGame(string path);
        void ExecuteGame();
    }
}
