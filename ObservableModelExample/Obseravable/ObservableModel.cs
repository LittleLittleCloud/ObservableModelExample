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
        public delegate Task NotifyDependencyNodeDelegateAsync();

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

        public void RegisterViewModel<T>(T value, [CallerMemberName] string propertyName = null)
            where T: ObservableModel
        {
            this.DependencyGraphManager.RegisterViewModel(value, propertyName);
        }

        public void UnegisterViewModel<T>(T value)
            where T : ObservableModel
        {
            this.DependencyGraphManager.UnregisterViewModel(value);
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
