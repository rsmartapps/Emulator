using Emulator.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Emulator.Player
{
    public class DebugWindowVM : WindowVM
    {
        #region registers
        private string _A;
        public string A
        {
            get { return _A; }
            set
            {
                if (value != _A)
                {
                    _A = value;
                    OnPropertyChanged(nameof(A));
                }
            }
        }

        private string _B;
        public string B
        {
            get { return _B; }
            set
            {
                if (value != _B)
                {
                    _B = value;
                    OnPropertyChanged(nameof(B));
                }
            }
        }

        private string _C;
        public string C
        {
            get { return _C; }
            set
            {
                if (value != _C)
                {
                    _C = value;
                    OnPropertyChanged(nameof(C));
                }
            }
        }

        private string _D;
        public string D
        {
            get { return _D; }
            set
            {
                if (value != _D)
                {
                    _D = value;
                    OnPropertyChanged(nameof(D));
                }
            }
        }

        private string _E;
        public string E
        {
            get { return _E; }
            set
            {
                if (value != _E)
                {
                    _E = value;
                    OnPropertyChanged(nameof(E));
                }
            }
        }

        private string _H;
        public string H
        {
            get { return _H; }
            set
            {
                if (value != _H)
                {
                    _H = value;
                    OnPropertyChanged(nameof(H));
                }
            }
        }

        private string _L;
        public string L
        {
            get { return _L; }
            set
            {
                if (value != _L)
                {
                    _L = value;
                    OnPropertyChanged(nameof(L));
                }
            }
        }

        private string _PC;
        public string PC
        {
            get { return _PC; }
            set
            {
                if (value != _PC)
                {
                    _PC = value;
                    OnPropertyChanged(nameof(PC));
                }
            }
        }

        private string _SP;
        public string SP
        {
            get { return _SP; }
            set
            {
                if (value != _SP)
                {
                    _SP = value;
                    OnPropertyChanged(nameof(SP));
                }
            }
        }
        #endregion
        #region Flags
        private string _Zero;
        public string Zero
        {
            get { return _Zero; }
            set
            {
                if (value != _Zero)
                {
                    _Zero = value;
                    OnPropertyChanged(nameof(Zero));
                }
            }
        }
        private string _Substract;
        public string Substract
        {
            get { return _Substract; }
            set
            {
                if (value != _Substract)
                {
                    _Substract = value;
                    OnPropertyChanged(nameof(Substract));
                }
            }
        }
        private string _Halt;
        public string Halt
        {
            get { return _Halt; }
            set
            {
                if (value != _Halt)
                {
                    _Halt = value;
                    OnPropertyChanged(nameof(Halt));
                }
            }
        }
        private string _Carry;
        public string Carry
        {
            get { return _Carry; }
            set
            {
                if (value != _Carry)
                {
                    _Carry = value;
                    OnPropertyChanged(nameof(Carry));
                }
            }
        }
        #endregion
        #region Interrupts

        private string _IME;
        public string IME
        {
            get { return _IME; }
            set
            {
                if (value != _IME)
                {
                    _IME = value;
                    OnPropertyChanged(nameof(IME));
                }
            }
        }
        #endregion
        private string _logs;
        public string Logs
        {
            get { return _logs; }
            set
            {
                if (value != _logs)
                {
                    _logs = value;
                    OnPropertyChanged(nameof(Logs));
                }
            }
        }

        private BitmapImage _BitmapVRAM;
        public BitmapImage BitmapVRAM
        {
            get { return _BitmapVRAM; }
            set
            {
                if (value != _BitmapVRAM)
                {
                    _BitmapVRAM = value;
                    OnPropertyChanged(nameof(BitmapVRAM));
                }
            }
        }
        private BitmapImage _BitmapORAM;
        public BitmapImage BitmapORAM
        {
            get { return _BitmapORAM; }
            set
            {
                if (value != _BitmapORAM)
                {
                    _BitmapORAM = value;
                    OnPropertyChanged(nameof(BitmapORAM));
                }
            }
        }
        public Pixel[,] VRAM = new Pixel[15, 186];
        public Pixel[,] VRAM2 = new Pixel[15, 186];
        public Pixel[,] ORAM = new Pixel[15, 10];

        public void Update()
        {
            UpdateRegisters();
            UpdateFlags();
            UpdateInterrupts();
            UpdateVRam();
            UpdateORam();
        }

        public string GetColor(bool value)
        {
            return value ? "Green" : "Red";
        }


        private void UpdateRegisters()
        {
        }

        private void UpdateFlags()
        {

        }

        private void UpdateInterrupts()
        {

        }

        private void UpdateVRam()
        {

        }

        private void UpdateORam()
        {

        }
    }
}
