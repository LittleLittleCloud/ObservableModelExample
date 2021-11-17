using FluentAssertions;
using ObservableModelExample.Obseravable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ObservableModelTests
{
    public class DependencyGraphManagerTest
    {
        [Fact]
        public void Dependency_graph_manager_register_naive_view_model_test()
        {
            var vm1 = new NaiveViewModel();
            var manager = vm1.DependencyGraphManager;

            manager.DependencyGraph.Count().Should().Be(7);
            manager.DependencyGraph.Contains(KeyValuePair.Create("this.C", "this.A")).Should().BeTrue();
            manager.DependencyGraph.Contains(KeyValuePair.Create("this.D", "this.A")).Should().BeTrue();
            manager.DependencyGraph.Contains(KeyValuePair.Create("this.D", "this.B")).Should().BeTrue();
            manager.DependencyGraph.Contains(KeyValuePair.Create("this.E", "this.A")).Should().BeTrue();
            manager.DependencyGraph.Contains(KeyValuePair.Create("this.E", "this.B")).Should().BeTrue();
            manager.DependencyGraph.Contains(KeyValuePair.Create("this.E", "this.C")).Should().BeTrue();
            manager.DependencyGraph.Contains(KeyValuePair.Create("this.E", "this.D")).Should().BeTrue();
        }

        [Fact]
        public async void Naive_view_mode_dependency_notify_test()
        {
            var vm1 = new NaiveViewModel();
            var observer = new List<string>();
            vm1.PropertyChanged += (sender, e) =>
            {
                if(e.PropertyName != null)
                    observer.Add(e.PropertyName);
            };

            vm1.A = "a";
            await vm1.WaitForDependencyUpdateCompleteAsync();
            observer.Should().Equal("A", "C", "D", "E");
            observer.Clear();

            vm1.B = "b";
            await vm1.WaitForDependencyUpdateCompleteAsync();
            observer.Should().Equal("B", "D", "E");
        }

        private class NaiveViewModel : ObservableModel
        {
            private string a;
            private string b;

            public NaiveViewModel()
                : base()
            {
            }

            public string A
            {
                get => this.a;
                set
                {
                    this.a = value;
                    this.NotifyPropertyChange();
                }
            }

            public string B
            {
                get => this.b;
                set
                {
                    this.b = value;
                    this.NotifyPropertyChange();
                }
            }

            [DependsOn(nameof(A))]
            public string C { get => this.A + "C"; }

            [DependsOn(nameof(A))]
            [DependsOn(nameof(B))]
            [DependsOn(nameof(C))]
            [DependsOn(nameof(D))]
            public string E { get => this.A + this.B + this.C + this.D; }

            [DependsOn(nameof(A))]
            [DependsOn(nameof(B))]
            public string D { get => this.A + this.B; }

        }

        private class NaiveViewModelWithUpdateHandler : ObservableModel
        {
            private string a;
            private string b;

            public NaiveViewModelWithUpdateHandler()
                : base()
            {
            }

            public string A
            {
                get => this.a;
                set
                {
                    this.a = value;
                    this.NotifyPropertyChange();
                }
            }

            public string B
            {
                get => this.b;
                set
                {
                    this.b = value;
                    this.NotifyPropertyChange();
                }
            }

            [DependsOn(nameof(A))]
            public string C { get => this.A + "C"; }

            [DependsOn(nameof(A))]
            [DependsOn(nameof(B))]
            [DependsOn(nameof(C))]
            [DependsOn(nameof(D))]
            public string E { get => this.A + this.B + this.C + this.D; }

            [DependsOn(nameof(A))]
            [DependsOn(nameof(B))]
            public string D { get => this.A + this.B; }

            [DependsOn(nameof(C))]
            public async Task TimeConsumingTask1Async()
            {
                await Task.Delay(5000);
                this.B = "B" + this.C;
            }
        }
    }
}
