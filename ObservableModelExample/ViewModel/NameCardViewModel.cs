using ObservableModelExample.Obseravable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObservableModelExample.ViewModel
{
    public class NameCardViewModel : ObservableModel
    {
        private string firstName;
        private string lastName;

        public string FirstName
        {
            get => this.firstName;
            set
            {
                this.firstName = value;
                this.NotifyPropertyChange();
            }
        }

        public string LastName
        {
            get => this.lastName;
            set
            {
                this.lastName = value;
                this.NotifyPropertyChange();
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
    }
}
