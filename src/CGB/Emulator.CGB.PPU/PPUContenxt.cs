using Emulator.CGB.Memory;
using Emulator.Domain.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator.CGB.PPU
{
    internal class PPUContenxt
    {
        #region LCD
        public const ushort LCD = 0xFF40;

        public byte data => memory.Read(LCD);

        public bool On => BitOps.IsBit(data, 1);
        public ushort WindowTileMapArea
        {
            get
            {
                if (BitOps.IsBit(data, 2))
                    return 0x9800;
                else
                    return 0x9C00;
            }
        }
        public bool WindowEnabled => BitOps.IsBit(data, 3);
        public ushort BGWindowTileDataArea
        {
            get
            {
                if (BitOps.IsBit(data, 2))
                    return 0x8800;
                else
                    return 0x8000;
            }
        }
        public ushort BGTileMapArea
        {
            get
            {
                if (BitOps.IsBit(data, 2))
                    return 0x9800;
                else
                    return 0x9C00;
            }
        }
        public bool OBJSize => BitOps.IsBit(data, 6);
        public bool OBJEnabled => BitOps.IsBit(data, 7);
        public bool BGWindowEnablePriority => BitOps.IsBit(data, 8);
        #endregion
        #region STAT

        public const ushort STATUS = 0xFF41;
        public const ushort LCD_Y = 0xFF44;
        public const ushort LYC_Y = 0xFF45;

        public byte StatusData
        {
            get => memory.Read(STATUS);
            set => memory.Write(STATUS, value);
        }
        public byte LCDY => memory.Read(LCD_Y);
        public byte LYCY { 
            get => memory.Read(LYC_Y); 
            set => memory.Write(LYC_Y, value); 
        }

        public LCDMode Mode
            => (BitOps.IsBit(StatusData, 1), BitOps.IsBit(StatusData, 0)) switch
        {
            (false, false) => LCDMode.HBlank,
            (false, true) => LCDMode.VBlank,
            (true, false) => LCDMode.OAM,
            (true, true) => LCDMode.Render
        };
        #endregion
        private ICGBMemoryBus memory;

        public PPUContenxt(ICGBMemoryBus memory)
        {
            this.memory = memory;
        }
    }
}
