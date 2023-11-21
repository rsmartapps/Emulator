using System;
using System.Windows;
using Emulator.CGB;
using Emulator.GBC;

namespace Emulator.Player;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    CGBMachine Machine { get; set; }
    public MainWindow()
    {
        InitializeComponent();
    }

    public async void LoadGame(object sender, RoutedEventArgs e)
    {
        try
        {

            //await Machine.LoadGame("B:\\Dev\\Emulators\\ROMs\\Pokemon - Yellow.gbc");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
