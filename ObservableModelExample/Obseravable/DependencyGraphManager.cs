using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ObservableModelExample.Obseravable
{
    public class DependencyGraphManager
    {
        private Task notifyPropertyChangedTask;
        private const string THIS_VM = "this";
        private Dictionary<string, NotifyDependencyNodeDelegateAsync> notifyDependencyNodeDelegates;

        public DependencyGraphManager(IObservableModel vm)
        {
            this.ViewModels = new Dictionary<string, IObservableModel>();
            this.DependencyGraph = new List<KeyValuePair<string, string>>();
            this.notifyDependencyNodeDelegates = new Dictionary<string, NotifyDependencyNodeDelegateAsync>();
            this.Initialize(vm);
        }

        public Dictionary<string, IObservableModel> ViewModels;

        public void Initialize(IObservableModel vm)
        {
            this.ViewModels[THIS_VM] = vm;

            // properties
            var properties = vm.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach(var property in properties)
            {
                var dependencies = property.GetCustomAttributes(typeof(DependsOnAttribute), false);
                if (dependencies.Length > 0)
                {
                    var from = this.CreateDependencyNodeName(THIS_VM, property.Name);
                    if (!this.notifyDependencyNodeDelegates.ContainsKey(property.Name))
                    {
                        this.notifyDependencyNodeDelegates.Add(property.Name, async () => await Task.Run(() => vm.OnPropertyChange(property.Name)));
                    }
                    foreach (var dependency in dependencies)
                    {
                        var dependencyPaths = (dependency as DependsOnAttribute).Properties;
                        var to = this.CreateDependencyNodeName(THIS_VM, dependencyPaths);
                        this.DependencyGraph.Add(KeyValuePair.Create(from, to));
                    }
                }
            }

            // update handler
            var methods = vm.GetType().GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach(var method in methods)
            {
                var from = this.CreateDependencyNodeName(THIS_VM, method.Name);
                var dependencies = method.GetCustomAttributes(typeof(DependsOnAttribute), false);
                if (dependencies.Length > 0)
                {
                    if (!this.notifyDependencyNodeDelegates.ContainsKey(method.Name))
                    {
                        var fun = (NotifyDependencyNodeDelegateAsync)method.CreateDelegate(typeof(NotifyDependencyNodeDelegateAsync), vm);
                        this.notifyDependencyNodeDelegates.Add(method.Name, async () => await fun());
                    }

                    foreach(var dependency in dependencies)
                    {
                        var dependencyPaths = (dependency as DependsOnAttribute).Properties;
                        var to = this.CreateDependencyNodeName(THIS_VM, dependencyPaths);
                        this.DependencyGraph.Add(KeyValuePair.Create(from, to));
                    }
                }

                var updates = method.GetCustomAttributes(typeof(UpdateAttribute), false);
                if (updates.Length > 0)
                {
                    if (!this.notifyDependencyNodeDelegates.ContainsKey(method.Name))
                    {
                        var fun = (NotifyDependencyNodeDelegateAsync)method.CreateDelegate(typeof(NotifyDependencyNodeDelegateAsync), vm);
                        this.notifyDependencyNodeDelegates.Add(method.Name, () => fun());
                    }

                    foreach (var update in updates)
                    {
                        var dependencyPath = (update as UpdateAttribute).Property;
                        var to = this.CreateDependencyNodeName(THIS_VM, dependencyPath);
                        this.DependencyGraph.Add(KeyValuePair.Create(to, from));

                        // if 'to' is thisVM's property, add notifyDependencyNodeDelegates.
                        if (!this.notifyDependencyNodeDelegates.ContainsKey(dependencyPath))
                        {
                            this.notifyDependencyNodeDelegates.Add(dependencyPath, () => Task.Run(() => vm.OnPropertyChange(dependencyPath)));
                        }
                    }
                }
            }
        }

        public void NotifyPropertyChange([CallerMemberName] string propertyName = null)
        {
            this.notifyPropertyChangedTask = Task.Run(async () =>
            {
                this.ViewModels[THIS_VM].OnPropertyChange(propertyName);
                await this.TryExecuteNotifyDependencyNodeDelegateAsync(propertyName);
            });
        }

        public async Task WaitForDependencyUpdateCompleteAsync()
        {
            if (this.notifyPropertyChangedTask != null)
            {
                await this.notifyPropertyChangedTask;
            }
        }

        public void RegisterViewModel<T>(T? vm, [CallerMemberName]string prefix="") where T: IObservableModel
        {
            if(vm == null)
            {
                return;
            }

            prefix = this.CreateDependencyNodeName(THIS_VM, prefix);
            var vmId = this.CreateViewModelId<T>();
            this.ViewModels[vmId] = vm;
            var dependencyNodes = vm.DependencyGraphManager.DependencyGraph.Where(kv => kv.Value.StartsWith(prefix));
            IEnumerable<KeyValuePair<string, string>> subGraph = new List<KeyValuePair<string, string>>();
            foreach(var dependencyNode in dependencyNodes)
            {
                vm.DependencyGraphManager.CalculateNodeDependencies(dependencyNode.Value, ref subGraph);
            }

            // register vm in vm.ViewModels
            foreach(var node in subGraph.SelectMany(kv => new[] {kv.Key, kv.Value }).Distinct())
            {
                var viewModelId = this.GetViewModelIdFromDependencyPath(node);
                var newViewModelId = viewModelId == THIS_VM ? vmId : $"{vmId}.{viewModelId}";
                if (!this.ViewModels.ContainsKey(newViewModelId) && vm.DependencyGraphManager.ViewModels.ContainsKey(viewModelId))
                {
                    this.ViewModels.Add(newViewModelId, vm.DependencyGraphManager.ViewModels[viewModelId]);
                }
            }

            foreach(var kv in subGraph)
            {
                string updateVMId(string node)
                {
                    if (node.Contains(prefix))
                    {
                        return node.Replace(prefix, THIS_VM);
                    }
                    else if(node.Contains(THIS_VM))
                    {
                        return node.Replace(THIS_VM, vmId);
                    }
                    else
                    {
                        return $"{vmId}.{node}";
                    }
                }

                this.DependencyGraph.Add(KeyValuePair.Create(updateVMId(kv.Key), updateVMId(kv.Value)));
            }
        }

        public void UnregisterViewModel<T>(T vm) where T: IObservableModel
        {
            var vmKv = this.ViewModels.Where(kv => kv.Value.Equals(vm)).FirstOrDefault();
            if(vmKv is KeyValuePair<string, IObservableModel> kv)
            {
                var vmId = kv.Key;
                this.ViewModels.Remove(kv.Key);
                this.DependencyGraph.RemoveAll(val => val.Key.StartsWith(vmId) || val.Value.StartsWith(vmId));
            }
        }

        private string CreateViewModelId<T>() where T : IObservableModel
        {
            var index = this.ViewModels.Keys.Where(k => k.Contains(typeof(T).Name)).Count();
            return $"{typeof(T).Name}_{index}";
        }

        public async Task<bool> TryExecuteNotifyDependencyNodeDelegateAsync(string node)
        {
            try
            {
                var prefix = this.CreateDependencyNodeName(THIS_VM, node);
                var dependencyNodes = this.DependencyGraph.Where(kv => kv.Value.StartsWith(prefix));
                IEnumerable<KeyValuePair<string, string>> subGraph = new List<KeyValuePair<string, string>>();
                foreach (var dependencyNode in dependencyNodes)
                {
                    this.CalculateNodeDependencies(dependencyNode.Value, ref subGraph);
                }

                var edges = subGraph.Select(x => new Tuple<string, string>(x.Value, x.Key)).Distinct().OrderBy(kv => kv.Item1).ToList();
                var nodes = subGraph.SelectMany(x => new[] { x.Key, x.Value }).Distinct().OrderBy(k => k).ToList();
                var sortedNodes = this.TopologicalSort(nodes, edges);
                foreach (var sortedNode in sortedNodes)
                {
                    var vmId = this.GetViewModelIdFromDependencyPath(sortedNode);
                    var dependencyFullPath = this.GetDependencyNameFromDependencyPath(sortedNode);
                    if(this.ViewModels.ContainsKey(vmId) && this.ViewModels[vmId].DependencyGraphManager.notifyDependencyNodeDelegates.TryGetValue(dependencyFullPath, out var fun))
                    {
                        await fun();
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void CalculateNodeDependencies(string node, ref IEnumerable<KeyValuePair<string, string>> graph)
        {
            // BFS
            var edges = this.DependencyGraph.Where(kv => kv.Value == node).ToList();
            
            if(edges.Count() == 0)
            {
                return;
            }

            graph = graph.Concat(edges).Distinct();
            foreach(var edge in edges)
            {
                this.CalculateNodeDependencies(edge.Key, ref graph);
            }
        }

        private List<T> TopologicalSort<T>(IEnumerable<T> nodes, List<Tuple<T, T>> edges) where T : IEquatable<T>
        {
            // Empty list that will contain the sorted elements
            var L = new List<T>();

            // Set of all nodes with no incoming edges
            var S = new HashSet<T>(nodes.Where(n => edges.All(e => e.Item2.Equals(n) == false)));

            // while S is non-empty do
            while (S.Any())
            {

                //  remove a node n from S
                var n = S.First();
                S.Remove(n);

                // add n to tail of L
                L.Add(n);

                // for each node m with an edge e from n to m do
                foreach (var e in edges.Where(e => e.Item1.Equals(n)).ToList())
                {
                    var m = e.Item2;

                    // remove edge e from the graph
                    edges.Remove(e);

                    // if m has no other incoming edges then
                    if (edges.All(me => me.Item2.Equals(m) == false))
                    {
                        // insert m into S
                        S.Add(m);
                    }
                }
            }

            // if graph has edges then
            if (edges.Any())
            {
                // return error (graph has at least one cycle)
                return null;
            }
            else
            {
                // return L (a topologically sorted order)
                return L;
            }
        }

        public List<KeyValuePair<string, string>> DependencyGraph { get; }

        private string CreateDependencyNodeName(string vmId, params string[] propertyNames) => $"{vmId}.{string.Join(".", propertyNames)}";

        private string GetViewModelIdFromDependencyPath(string path) => string.Join(".", path.Split(".").SkipLast(1));

        private string GetDependencyNameFromDependencyPath(string path) => path.Split('.').Last();
    }
}
