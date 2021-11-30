using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObservableModelExample.ViewModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ObservableModel.CodeGenerator;

namespace ObservableModelExample
{
    public partial class MainWindowViewModel: IObservableModel
    {
        public MainWindowViewModel()
        {
            this.DependencyGraphManager = new DependencyGraphManager(this);
        }

        public NameCardViewModel NameCard
        {
            get => this.nameCard;
            set
            {
                this.nameCard?.DependencyGraphManager.UnregisterViewModel(this);
                this.nameCard = value;
                value.DependencyGraphManager.RegisterViewModel(this);
                this.DependencyGraphManager.NotifyPropertyChange();
            }
        }

        public DependencyGraphManager DependencyGraphManager { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChange([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
