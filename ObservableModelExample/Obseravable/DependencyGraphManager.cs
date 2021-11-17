using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObservableModelExample.Obseravable
{
    public class DependencyGraphManager
    {
        private const string THIS_VM = "this";
        private delegate Task NotifyDependencyNodeDelegateAsync();
        private Dictionary<string, NotifyDependencyNodeDelegateAsync> notifyDependencyNodeDelegates;
        private Dictionary<string, ObservableModel> vms;

        public DependencyGraphManager(ObservableModel vm)
        {
            this.vms = new Dictionary<string, ObservableModel>();
            this.DependencyGraph = new List<KeyValuePair<string, string>>();
            this.notifyDependencyNodeDelegates = new Dictionary<string, NotifyDependencyNodeDelegateAsync>();
            this.RegisterViewModel(vm);
        }

        public void RegisterViewModel(ObservableModel vm)
        {
            this.vms[THIS_VM] = vm;
            var properties = vm.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach(var property in properties)
            {
                var dependencies = property.GetCustomAttributes(typeof(DependsOnAttribute), false);
                if (dependencies.Length > 0)
                {
                    var from = this.CreateDependencyNodeName(THIS_VM, property.Name);
                    if (!this.notifyDependencyNodeDelegates.ContainsKey(from))
                    {
                        this.notifyDependencyNodeDelegates.Add(from, () => Task.Run(() => vm.OnPropertyChange(property.Name)));
                    }
                    foreach (var dependency in dependencies)
                    {
                        var dependencyPaths = (dependency as DependsOnAttribute).Properties;
                        var to = this.CreateDependencyNodeName(THIS_VM, dependencyPaths);
                        this.DependencyGraph.Add(KeyValuePair.Create(from, to));

                        // if 'to' is thisVM's property, add notifyDependencyNodeDelegates.
                        if (dependencyPaths.Count() == 1 && !this.notifyDependencyNodeDelegates.ContainsKey(to))
                        {
                            this.notifyDependencyNodeDelegates.Add(to, () => Task.Run(() => vm.OnPropertyChange(dependencyPaths[0])));
                        }

                    }
                }
            }
        }

        public async Task<bool> TryExecuteNotifyDependencyNodeDelegateAsync(string node)
        {
            try
            {
                node = this.CreateDependencyNodeName(THIS_VM, node);
                IEnumerable<KeyValuePair<string, string>> subGraph = new List<KeyValuePair<string, string>>();
                this.CalculateNodeDependencies(node, ref subGraph);

                var edges = subGraph.Select(x => new Tuple<string, string>(x.Value, x.Key)).ToHashSet();
                var nodes = subGraph.SelectMany(x => new[] { x.Key, x.Value }).ToHashSet();
                var sortedNodes = this.TopologicalSort(nodes, edges);
                foreach (var sortedNode in sortedNodes)
                {
                    await this.notifyDependencyNodeDelegates[sortedNode]();
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
            var edges = this.DependencyGraph.Where(kv => kv.Value == node);
            
            if(edges.Count() == 0)
            {
                return;
            }

            graph = graph.Concat(edges);
            foreach(var edge in edges)
            {
                this.CalculateNodeDependencies(edge.Key, ref graph);
            }
        }

        private List<T> TopologicalSort<T>(HashSet<T> nodes, HashSet<Tuple<T, T>> edges) where T : IEquatable<T>
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
    }
}
