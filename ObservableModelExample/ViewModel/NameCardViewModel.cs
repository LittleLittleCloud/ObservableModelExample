using ObservableModelExample.Obseravable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ObservableModelExample.ViewModel
{
    public class NameCardViewModel : IObservableModel
    {
        private string firstName;
        private string lastName;

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

        [DependsOn(nameof(LastName))]
        [DependsOn(nameof(FirstName))]
        public string FullName
        {
            get
            {
                if (this.FirstName is string && this.LastName is string)
                {
                    return this.FirstName + " " + this.LastName;
                }

                return string.Empty;
            }
        }

        public string Description
        {
            get; set;
        }

        public DependencyGraphManager DependencyGraphManager { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        [DependsOn(nameof(FullName))]
        [Update(nameof(Description))]
        public async Task FetchDescriptionFromBackgroundAsync()
        {
            if(this.FullName is string)
            {
                this.Description = "pull description...";
                await Task.Delay(5000);
                this.Description = $"{this.FullName} doesn't have any description....";
            }
            
        }

        public void OnPropertyChange([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
