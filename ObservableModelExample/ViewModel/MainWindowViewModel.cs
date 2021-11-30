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
    public partial class MainWindowViewModel
    {
        private NameCardViewModel nameCard;

        [DependsOn(nameof(NameCard), nameof(NameCardViewModel.FullName))]
        [DependsOn(nameof(NameCard), nameof(NameCardViewModel.Description))]
        public string OtherInformation
        {
            get
            {
                if(this.NameCard?.FullName is string && this.NameCard?.Description is string)
                {
                    return $"Current Selection: {this.NameCard.FullName}, Description: {this.NameCard.Description}";
                }

                return null;
            }
        }
    }
}
