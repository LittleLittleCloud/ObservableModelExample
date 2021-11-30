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
    public partial class NameCardViewModel
    {
        private string firstName;
        private string lastName;
        private string description;

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

        [DependsOn(nameof(FullName))]
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
