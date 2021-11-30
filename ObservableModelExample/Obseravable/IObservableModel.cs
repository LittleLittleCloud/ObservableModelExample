using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ObservableModelExample.Obseravable
{
    public interface IObservableModel: INotifyPropertyChanged
    {
        DependencyGraphManager DependencyGraphManager { get; }

        void OnPropertyChange([CallerMemberName]string propertyName = null);
    }
}