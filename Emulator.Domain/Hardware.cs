using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.Domain
{
    public class Hardware
    {
        public static MachineMemory Memory = new MachineMemory();
        public static CPU CPU = new CPU();
    }
}
