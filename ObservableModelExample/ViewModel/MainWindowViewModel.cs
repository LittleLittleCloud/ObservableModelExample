using ObservableModelExample.Obseravable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObservableModelExample.ViewModel;

namespace ObservableModelExample
{
    public class MainWindowViewModel: ObservableModel
    {
        private NameCardViewModel nameCard;

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
