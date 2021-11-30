using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ObservableModel.CodeGenerator;
using System.Threading.Tasks;

namespace ObservableModelExample.ViewModel
{
    public partial class NameCardViewModel : IObservableModel
    {
        public NameCardViewModel()
        {
            this.DependencyGraphManager = new DependencyGraphManager(this);
        }
        

        public string FirstName
        {
            get => this.firstName;
            set
            {
                this.firstName = value;
                this.DependencyGraphManager.NotifyPropertyChange();
            }
        }

        public string LastName
        {
            get => this.lastName;
            set
            {
                this.lastName = value;
                this.DependencyGraphManager.NotifyPropertyChange();
            }
        }

        public string Description
        {
            get => this.description;
            set
            {
                this.description = value;
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
