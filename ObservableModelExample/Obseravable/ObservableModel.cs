using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ObservableModelExample.Obseravable
{
    public abstract class ObservableModel : INotifyPropertyChanged
    {
        private Task notifyPropertyChangeTask;

        public ObservableModel()
        {
            this.DependencyGraphManager = new DependencyGraphManager(this);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DependencyGraphManager DependencyGraphManager { get; }

        protected void NotifyPropertyChange([CallerMemberName]string propertyName = null)
        {
            this.notifyPropertyChangeTask = Task.Run(async () =>
            {
                await this.DependencyGraphManager.TryExecuteNotifyDependencyNodeDelegateAsync(propertyName);
            });
        }

        public void OnPropertyChange(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task WaitForDependencyUpdateCompleteAsync()
        {
            if (this.notifyPropertyChangeTask != null)
            {
                await this.notifyPropertyChangeTask;
            }
        }
    }
}
