using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ObservableModelExample.Obseravable
{
    public interface IObservableModel: INotifyPropertyChanged
    {
        DependencyGraphManager DependencyGraphManager { get; }

        void OnPropertyChange([CallerMemberName]string propertyName = null);
    }
}