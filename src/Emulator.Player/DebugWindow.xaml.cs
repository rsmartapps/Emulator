using Emulator.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Emulator.Player
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        Timer timer;
        DebugWindowVM vm;

        public DebugWindow()
        {
            InitializeComponent();
            vm = new DebugWindowVM();
            DataContext = vm;
            timer = new Timer(500);
            timer.Elapsed += UpdateDebugWIndow;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        int counter = 0;
        private void UpdateDebugWIndow(object? sender, ElapsedEventArgs e)
        {
            counter++;
            try
            {
                Dispatcher.Invoke(() =>
                {
                    try {
                        vm.BitmapVRAM = new BitmapImage();
                        vm.BitmapORAM = new BitmapImage();
                        vm.Logs = $"Counter count {counter}";
                        UpdateLayout();
                        vm.Zero = vm.GetColor(true);
                        vm.Substract = vm.GetColor(false);
                        vm.Halt = vm.GetColor(true);
                        vm.Carry = vm.GetColor(false);
                        vm.IME = vm.GetColor(true);

                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            catch(Exception ex)
            {

            }
        }
    }
}
