using FluentAssertions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ObservableModel.CodeGenerator;
using Xunit;

namespace ObservableModelTests
{
    public class DependencyGraphManagerTest
    {
        [Fact]
        public void Naive_view_model_dependency_graph_test()
        {
            var vm1 = new NaiveViewModel();
            var manager = vm1.DependencyGraphManager;

            manager.DependencyGraph.Count().Should().Be(3);
            manager.DependencyGraph.Contains(KeyValuePair.Create("this.C", "this.A")).Should().BeTrue();
            manager.DependencyGraph.Contains(KeyValuePair.Create("this.B", "this.A")).Should().BeTrue();
            manager.DependencyGraph.Contains(KeyValuePair.Create("this.C", "this.B")).Should().BeTrue();
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
            await vm1.DependencyGraphManager.WaitForDependencyUpdateCompleteAsync();
            observer.Should().Equal("A", "B", "C");
        }

        [Fact]
        public void Nest_view_model_dependency_graph_test()
        {
            var vm = new NestViewModel();
            var manager = vm.DependencyGraphManager;

            manager.DependencyGraph.Count().Should().Be(2);
            manager.DependencyGraph.Contains(KeyValuePair.Create("this.B", "this.NestVM.B")).Should().BeTrue();
            manager.DependencyGraph.Contains(KeyValuePair.Create("this.B", "this.NestVM.NestVM.B")).Should().BeTrue();

            vm.NestVM = new NestViewModel();
            vm.NestVM.DependencyGraphManager.DependencyGraph.Count().Should().Be(4);
            vm.NestVM.DependencyGraphManager.DependencyGraph.Contains(KeyValuePair.Create("this.B", "this.NestVM.B")).Should().BeTrue();
            vm.NestVM.DependencyGraphManager.DependencyGraph.Contains(KeyValuePair.Create("this.B", "this.NestVM.NestVM.B")).Should().BeTrue();
            vm.NestVM.DependencyGraphManager.DependencyGraph.Contains(KeyValuePair.Create("NestViewModel_0.B", "this.B")).Should().BeTrue();
            vm.NestVM.DependencyGraphManager.DependencyGraph.Contains(KeyValuePair.Create("NestViewModel_0.B", "this.NestVM.B")).Should().BeTrue();

            vm.NestVM.NestVM = new NestViewModel();
            vm.NestVM.NestVM.DependencyGraphManager.DependencyGraph.Count().Should().Be(6);

            vm.NestVM.NestVM.DependencyGraphManager.DependencyGraph.Contains(KeyValuePair.Create("this.B", "this.NestVM.B")).Should().BeTrue();
            vm.NestVM.NestVM.DependencyGraphManager.DependencyGraph.Contains(KeyValuePair.Create("this.B", "this.NestVM.NestVM.B")).Should().BeTrue();
            vm.NestVM.NestVM.DependencyGraphManager.DependencyGraph.Contains(KeyValuePair.Create("NestViewModel_0.B", "this.B")).Should().BeTrue();
            vm.NestVM.NestVM.DependencyGraphManager.DependencyGraph.Contains(KeyValuePair.Create("NestViewModel_0.B", "this.NestVM.B")).Should().BeTrue();
            vm.NestVM.NestVM.DependencyGraphManager.DependencyGraph.Contains(KeyValuePair.Create("NestViewModel_0.NestViewModel_0.B", "this.B")).Should().BeTrue();
            vm.NestVM.NestVM.DependencyGraphManager.DependencyGraph.Contains(KeyValuePair.Create("NestViewModel_0.NestViewModel_0.B", "NestViewModel_0.B")).Should().BeTrue();
        }

        [Fact]
        public async void Nest_view_model_dependency_notify_test()
        {
            var vm = new NestViewModel();
            var observer = new List<string>();
            vm.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    observer.Add(e.PropertyName);
            };

            vm.NestVM = new NestViewModel();
            await vm.DependencyGraphManager.WaitForDependencyUpdateCompleteAsync();

            observer.Should().Equal("NestVM", "B");
            observer.Clear();

            vm.NestVM.NestVM = new NestViewModel();
            await vm.NestVM.DependencyGraphManager.WaitForDependencyUpdateCompleteAsync();
            observer.Should().Equal("B");
            observer.Clear();

            vm.NestVM.NestVM.NestVM = new NestViewModel();
            await vm.NestVM.NestVM.DependencyGraphManager.WaitForDependencyUpdateCompleteAsync();
            observer.Should().Equal("B");
        }

        [Fact]
        public async void Nest_view_model_unregister_test()
        {
            var vm = new NestViewModel();
            var vm2 = new NestViewModel();
            var observer = new List<string>();
            vm.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    observer.Add(e.PropertyName);
            };

            // register vm2 through setter
            vm.NestVM = vm2;
            await vm.DependencyGraphManager.WaitForDependencyUpdateCompleteAsync();

            observer.Should().Equal("NestVM", "B");
            observer.Clear();

            // unregister vm by setting vm.NestVM to null (or another vm)
            vm.NestVM = null;
            await vm.DependencyGraphManager.WaitForDependencyUpdateCompleteAsync();
            observer.Should().Equal("NestVM", "B");
            observer.Clear();

            // since it's unregistered, updating vm2 won't trigger notification in vm.
            vm2.NestVM = new NestViewModel();
            await vm2.DependencyGraphManager.WaitForDependencyUpdateCompleteAsync();
            observer.Should().BeEmpty();
        }
    }

    public partial class NaiveViewModel
    {
        [AutoNotify]
        private string a;

        [DependsOn(nameof(A))]
        [DependsOn(nameof(B))]
        public string C { get => this.A + this.B + "C"; }

        [DependsOn(nameof(A))]
        public string B { get => this.A + "B"; }
    }

    public partial class NestViewModel
    {
        [AutoNotify]
        private NestViewModel nestVM;

        [DependsOn(nameof(NestVM), nameof(NestVM), nameof(B))]
        [DependsOn(nameof(NestVM), nameof(B))]
        public string B
        {
            get => this.NestVM.NestVM.B + this.NestVM.B + "B";
        }
    }
}
