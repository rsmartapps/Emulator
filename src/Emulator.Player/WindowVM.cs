using Emulator.Domain;
using System.ComponentModel;

namespace Emulator.Player;

public class WindowVM : INotifyPropertyChanged
{
    public IMachine Machine { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
