using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows;
using Emulator.GBC;

namespace Emulator.Player
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        DebugWindowVM vm;
        Thread thread;

        public DebugWindow()
        {
            InitializeComponent();
            vm = new DebugWindowVM();
            DataContext = vm;
            vm.BitmapVRAM = new Bitmap(256, 256);
            this.Loaded += WindowLoaded;
            thread = new Thread(UpdateDebugWIndow);
            thread.Start();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadGame("B:\\Dev\\Emulators\\ROMs\\Tennis.gb");
        }

        private void UpdateDebugWIndow()
        {
           while(true)
            {
                try
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            vm.Update();
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                }
                catch (Exception ex)
                {

                }
                Thread.Sleep(2000);
            }
        }

        public void LoadGame(string path)
        {
            vm.Machine = new CGBMachine();
            vm.machine.InsertCartRidge(path);
            vm.Start();
        }
    }
}
