using System.ComponentModel;
using System.Threading.Tasks;

namespace ObservableModelExample.Obseravable
{
    public interface IObservableModel
    {
        DependencyGraphManager DependencyGraphManager { get; }

        event PropertyChangedEventHandler? PropertyChanged;

        void OnPropertyChange(string propertyName = null);
    }
}