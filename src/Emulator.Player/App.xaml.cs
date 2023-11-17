using System.Windows;

namespace Emulator.Player
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //this.StartupUri = new System.Uri("MainWindow.xaml", System.UriKind.Relative);
            this.StartupUri = new System.Uri("DebugWindow.xaml", System.UriKind.Relative);
        }
    }
}
