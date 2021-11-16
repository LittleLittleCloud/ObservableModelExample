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
        private TaskFactory taskFactory = new TaskFactory();

        public ObservableModel()
        {
            this.ID = Guid.NewGuid().ToString();
            this.DependencyGraphManager = new DependencyGraphManager(this);
        }

        public ObservableModel(string id)
        {
            this.ID = id;
            this.DependencyGraphManager = new DependencyGraphManager(this);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DependencyGraphManager DependencyGraphManager { get; }
        
        public string ID { get; }

        protected void NotifyPropertyChange([CallerMemberName]string propertyName = null)
        {
            this.taskFactory.StartNew(async () =>
            {
                this.OnPropertyChange(propertyName);
                await this.DependencyGraphManager.TryExecuteNotifyDependencyNodeDelegateAsync(propertyName);
            });
        }

        public void OnPropertyChange(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
