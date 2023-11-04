using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.Domain
{
    public class MachineMemory
    {
        private Dictionary<ushort, byte> ROM = new(0xFFFFF);
        public virtual ushort ReadWord(ushort address)
        {
            ROM.TryGetValue(address, out byte value);
            return value;
        }
        public virtual byte Read(ushort address)
        {
            ROM.TryGetValue(address, out byte value);
            return value;
        }
        public virtual void Write(ushort address, byte value)
        {
            ROM.TryAdd(address, value);
        }

        public virtual void Load(byte[] file)
        {
            ushort address = 0;
            foreach (var bt in file)
            {
                ROM.TryAdd(address, bt);
                address++;
            }
        }
    }
}
