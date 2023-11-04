using Emulator.Domain;
using Emulator.GBC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Emulator.Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IMachine Machine { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Machine = new GBCMachine();
        }

        public async void LoadGame(object sender, RoutedEventArgs e)
        {
            try
            {

                await Machine.LoadGame("B:\\Dev\\Emulators\\ROMs\\Pokemon - Yellow.gbc");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
