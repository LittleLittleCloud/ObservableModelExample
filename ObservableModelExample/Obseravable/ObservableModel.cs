using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ObservableModelExample.Obseravable
{
    public delegate Task NotifyDependencyNodeDelegateAsync();

    public abstract class ObservableModel : IObservableModel
    {
        public ObservableModel()
        {
            this.DependencyGraphManager = new DependencyGraphManager(this);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DependencyGraphManager DependencyGraphManager { get; }

        public void OnPropertyChange(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
