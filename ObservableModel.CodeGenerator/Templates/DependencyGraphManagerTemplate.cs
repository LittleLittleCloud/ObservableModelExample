﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 17.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace ObservableModel.CodeGenerator.Templates
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    internal partial class DependencyGraphManagerTemplate : DependencyGraphManagerTemplateBase
    {
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write("﻿");
            this.Write("\r\nusing System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing Sys" +
                    "tem.Runtime.CompilerServices;\r\nusing System.Text;\r\nusing System.Threading.Tasks;" +
                    "\r\n\r\nnamespace ObservableModel.CodeGenerator\r\n{\r\n    public delegate Task NotifyD" +
                    "ependencyNodeDelegateAsync();\r\n\r\n    public class DependencyGraphManager\r\n    {\r" +
                    "\n        private Task notifyPropertyChangedTask;\r\n        private const string T" +
                    "HIS_VM = \"this\";\r\n        private Dictionary<string, NotifyDependencyNodeDelegat" +
                    "eAsync> notifyDependencyNodeDelegates;\r\n\r\n        public DependencyGraphManager(" +
                    "IObservableModel vm)\r\n        {\r\n            this.ViewModels = new Dictionary<st" +
                    "ring, IObservableModel>();\r\n            this.DependencyGraph = new List<KeyValue" +
                    "Pair<string, string>>();\r\n            this.notifyDependencyNodeDelegates = new D" +
                    "ictionary<string, NotifyDependencyNodeDelegateAsync>();\r\n            this.Initia" +
                    "lize(vm);\r\n        }\r\n\r\n        public Dictionary<string, IObservableModel> View" +
                    "Models;\r\n\r\n        public void Initialize(IObservableModel vm)\r\n        {\r\n     " +
                    "       this.ViewModels[THIS_VM] = vm;\r\n\r\n            // properties\r\n            " +
                    "var properties = vm.GetType().GetProperties(System.Reflection.BindingFlags.Publi" +
                    "c | System.Reflection.BindingFlags.Instance);\r\n            foreach(var property " +
                    "in properties)\r\n            {\r\n                var dependencies = property.GetCu" +
                    "stomAttributes(typeof(DependsOnAttribute), false);\r\n                if (dependen" +
                    "cies.Length > 0)\r\n                {\r\n                    var from = this.CreateD" +
                    "ependencyNodeName(THIS_VM, property.Name);\r\n                    if (!this.notify" +
                    "DependencyNodeDelegates.ContainsKey(property.Name))\r\n                    {\r\n    " +
                    "                    this.notifyDependencyNodeDelegates.Add(property.Name, async " +
                    "() => await Task.Run(() => vm.OnPropertyChange(property.Name)));\r\n              " +
                    "      }\r\n                    foreach (var dependency in dependencies)\r\n         " +
                    "           {\r\n                        var dependencyPaths = (dependency as Depen" +
                    "dsOnAttribute).Properties;\r\n                        var to = this.CreateDependen" +
                    "cyNodeName(THIS_VM, dependencyPaths);\r\n                        this.DependencyGr" +
                    "aph.Add(KeyValuePair.Create(from, to));\r\n                    }\r\n                " +
                    "}\r\n            }\r\n\r\n            // update handler\r\n            var methods = vm." +
                    "GetType().GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.B" +
                    "indingFlags.Instance);\r\n            foreach(var method in methods)\r\n            " +
                    "{\r\n                var from = this.CreateDependencyNodeName(THIS_VM, method.Name" +
                    ");\r\n                var dependencies = method.GetCustomAttributes(typeof(Depends" +
                    "OnAttribute), false);\r\n                if (dependencies.Length > 0)\r\n           " +
                    "     {\r\n                    if (!this.notifyDependencyNodeDelegates.ContainsKey(" +
                    "method.Name))\r\n                    {\r\n                        var fun = (NotifyD" +
                    "ependencyNodeDelegateAsync)method.CreateDelegate(typeof(NotifyDependencyNodeDele" +
                    "gateAsync), vm);\r\n                        this.notifyDependencyNodeDelegates.Add" +
                    "(method.Name, async () => await fun());\r\n                    }\r\n\r\n              " +
                    "      foreach(var dependency in dependencies)\r\n                    {\r\n          " +
                    "              var dependencyPaths = (dependency as DependsOnAttribute).Propertie" +
                    "s;\r\n                        var to = this.CreateDependencyNodeName(THIS_VM, depe" +
                    "ndencyPaths);\r\n                        this.DependencyGraph.Add(KeyValuePair.Cre" +
                    "ate(from, to));\r\n                    }\r\n                }\r\n\r\n                var" +
                    " updates = method.GetCustomAttributes(typeof(UpdateAttribute), false);\r\n        " +
                    "        if (updates.Length > 0)\r\n                {\r\n                    if (!thi" +
                    "s.notifyDependencyNodeDelegates.ContainsKey(method.Name))\r\n                    {" +
                    "\r\n                        var fun = (NotifyDependencyNodeDelegateAsync)method.Cr" +
                    "eateDelegate(typeof(NotifyDependencyNodeDelegateAsync), vm);\r\n                  " +
                    "      this.notifyDependencyNodeDelegates.Add(method.Name, () => fun());\r\n       " +
                    "             }\r\n\r\n                    foreach (var update in updates)\r\n         " +
                    "           {\r\n                        var dependencyPath = (update as UpdateAttr" +
                    "ibute).Property;\r\n                        var to = this.CreateDependencyNodeName" +
                    "(THIS_VM, dependencyPath);\r\n                        this.DependencyGraph.Add(Key" +
                    "ValuePair.Create(to, from));\r\n\r\n                        // if \'to\' is thisVM\'s p" +
                    "roperty, add notifyDependencyNodeDelegates.\r\n                        if (!this.n" +
                    "otifyDependencyNodeDelegates.ContainsKey(dependencyPath))\r\n                     " +
                    "   {\r\n                            this.notifyDependencyNodeDelegates.Add(depende" +
                    "ncyPath, () => Task.Run(() => vm.OnPropertyChange(dependencyPath)));\r\n          " +
                    "              }\r\n                    }\r\n                }\r\n            }\r\n      " +
                    "  }\r\n\r\n        public void NotifyPropertyChange([CallerMemberName] string proper" +
                    "tyName = null)\r\n        {\r\n            this.notifyPropertyChangedTask = Task.Run" +
                    "(async () =>\r\n            {\r\n                this.ViewModels[THIS_VM].OnProperty" +
                    "Change(propertyName);\r\n                await this.TryExecuteNotifyDependencyNode" +
                    "DelegateAsync(propertyName);\r\n            });\r\n        }\r\n\r\n        public async" +
                    " Task WaitForDependencyUpdateCompleteAsync()\r\n        {\r\n            if (this.no" +
                    "tifyPropertyChangedTask != null)\r\n            {\r\n                await this.noti" +
                    "fyPropertyChangedTask;\r\n            }\r\n        }\r\n\r\n        public void Register" +
                    "ViewModel<T>(T? vm, [CallerMemberName]string prefix=\"\") where T: IObservableMode" +
                    "l\r\n        {\r\n            if(vm == null)\r\n            {\r\n                return;" +
                    "\r\n            }\r\n\r\n            prefix = this.CreateDependencyNodeName(THIS_VM, p" +
                    "refix);\r\n            var vmId = this.CreateViewModelId<T>();\r\n            this.V" +
                    "iewModels[vmId] = vm;\r\n            var dependencyNodes = vm.DependencyGraphManag" +
                    "er.DependencyGraph.Where(kv => kv.Value.StartsWith(prefix));\r\n            IEnume" +
                    "rable<KeyValuePair<string, string>> subGraph = new List<KeyValuePair<string, str" +
                    "ing>>();\r\n            foreach(var dependencyNode in dependencyNodes)\r\n          " +
                    "  {\r\n                vm.DependencyGraphManager.CalculateNodeDependencies(depende" +
                    "ncyNode.Value, ref subGraph);\r\n            }\r\n\r\n            // register vm in vm" +
                    ".ViewModels\r\n            foreach(var node in subGraph.SelectMany(kv => new[] {kv" +
                    ".Key, kv.Value }).Distinct())\r\n            {\r\n                var viewModelId = " +
                    "this.GetViewModelIdFromDependencyPath(node);\r\n                var newViewModelId" +
                    " = viewModelId == THIS_VM ? vmId : $\"{vmId}.{viewModelId}\";\r\n                if " +
                    "(!this.ViewModels.ContainsKey(newViewModelId) && vm.DependencyGraphManager.ViewM" +
                    "odels.ContainsKey(viewModelId))\r\n                {\r\n                    this.Vie" +
                    "wModels.Add(newViewModelId, vm.DependencyGraphManager.ViewModels[viewModelId]);\r" +
                    "\n                }\r\n            }\r\n\r\n            foreach(var kv in subGraph)\r\n  " +
                    "          {\r\n                string updateVMId(string node)\r\n                {\r\n" +
                    "                    if (node.Contains(prefix))\r\n                    {\r\n         " +
                    "               return node.Replace(prefix, THIS_VM);\r\n                    }\r\n   " +
                    "                 else if(node.Contains(THIS_VM))\r\n                    {\r\n       " +
                    "                 return node.Replace(THIS_VM, vmId);\r\n                    }\r\n   " +
                    "                 else\r\n                    {\r\n                        return $\"{" +
                    "vmId}.{node}\";\r\n                    }\r\n                }\r\n\r\n                this" +
                    ".DependencyGraph.Add(KeyValuePair.Create(updateVMId(kv.Key), updateVMId(kv.Value" +
                    ")));\r\n            }\r\n        }\r\n\r\n        public void UnregisterViewModel<T>(T v" +
                    "m) where T: IObservableModel\r\n        {\r\n            var vmKv = this.ViewModels." +
                    "Where(kv => kv.Value.Equals(vm)).FirstOrDefault();\r\n            if(vmKv is KeyVa" +
                    "luePair<string, IObservableModel> kv)\r\n            {\r\n                var vmId =" +
                    " kv.Key;\r\n                this.ViewModels.Remove(kv.Key);\r\n                this." +
                    "DependencyGraph.RemoveAll(val => val.Key.StartsWith(vmId) || val.Value.StartsWit" +
                    "h(vmId));\r\n            }\r\n        }\r\n\r\n        private string CreateViewModelId<" +
                    "T>() where T : IObservableModel\r\n        {\r\n            var index = this.ViewMod" +
                    "els.Keys.Where(k => k.Contains(typeof(T).Name)).Count();\r\n            return $\"{" +
                    "typeof(T).Name}_{index}\";\r\n        }\r\n\r\n        public async Task<bool> TryExecu" +
                    "teNotifyDependencyNodeDelegateAsync(string node)\r\n        {\r\n            try\r\n  " +
                    "          {\r\n                var prefix = this.CreateDependencyNodeName(THIS_VM," +
                    " node);\r\n                var dependencyNodes = this.DependencyGraph.Where(kv => " +
                    "kv.Value.StartsWith(prefix));\r\n                IEnumerable<KeyValuePair<string, " +
                    "string>> subGraph = new List<KeyValuePair<string, string>>();\r\n                f" +
                    "oreach (var dependencyNode in dependencyNodes)\r\n                {\r\n             " +
                    "       this.CalculateNodeDependencies(dependencyNode.Value, ref subGraph);\r\n    " +
                    "            }\r\n\r\n                var edges = subGraph.Select(x => new Tuple<stri" +
                    "ng, string>(x.Value, x.Key)).Distinct().OrderBy(kv => kv.Item1).ToList();\r\n     " +
                    "           var nodes = subGraph.SelectMany(x => new[] { x.Key, x.Value }).Distin" +
                    "ct().OrderBy(k => k).ToList();\r\n                var sortedNodes = this.Topologic" +
                    "alSort(nodes, edges);\r\n                foreach (var sortedNode in sortedNodes)\r\n" +
                    "                {\r\n                    var vmId = this.GetViewModelIdFromDepende" +
                    "ncyPath(sortedNode);\r\n                    var dependencyFullPath = this.GetDepen" +
                    "dencyNameFromDependencyPath(sortedNode);\r\n                    if(this.ViewModels" +
                    ".ContainsKey(vmId) && this.ViewModels[vmId].DependencyGraphManager.notifyDepende" +
                    "ncyNodeDelegates.TryGetValue(dependencyFullPath, out var fun))\r\n                " +
                    "    {\r\n                        await fun();\r\n                    }\r\n            " +
                    "    }\r\n\r\n                return true;\r\n            }\r\n            catch (Excepti" +
                    "on)\r\n            {\r\n                return false;\r\n            }\r\n        }\r\n\r\n " +
                    "       private void CalculateNodeDependencies(string node, ref IEnumerable<KeyVa" +
                    "luePair<string, string>> graph)\r\n        {\r\n            // BFS\r\n            var " +
                    "edges = this.DependencyGraph.Where(kv => kv.Value == node).ToList();\r\n          " +
                    "  \r\n            if(edges.Count() == 0)\r\n            {\r\n                return;\r\n" +
                    "            }\r\n\r\n            graph = graph.Concat(edges).Distinct();\r\n          " +
                    "  foreach(var edge in edges)\r\n            {\r\n                this.CalculateNodeD" +
                    "ependencies(edge.Key, ref graph);\r\n            }\r\n        }\r\n\r\n        private L" +
                    "ist<T> TopologicalSort<T>(IEnumerable<T> nodes, List<Tuple<T, T>> edges) where T" +
                    " : IEquatable<T>\r\n        {\r\n            // Empty list that will contain the sor" +
                    "ted elements\r\n            var L = new List<T>();\r\n\r\n            // Set of all no" +
                    "des with no incoming edges\r\n            var S = new HashSet<T>(nodes.Where(n => " +
                    "edges.All(e => e.Item2.Equals(n) == false)));\r\n\r\n            // while S is non-e" +
                    "mpty do\r\n            while (S.Any())\r\n            {\r\n\r\n                //  remov" +
                    "e a node n from S\r\n                var n = S.First();\r\n                S.Remove(" +
                    "n);\r\n\r\n                // add n to tail of L\r\n                L.Add(n);\r\n\r\n     " +
                    "           // for each node m with an edge e from n to m do\r\n                for" +
                    "each (var e in edges.Where(e => e.Item1.Equals(n)).ToList())\r\n                {\r" +
                    "\n                    var m = e.Item2;\r\n\r\n                    // remove edge e fr" +
                    "om the graph\r\n                    edges.Remove(e);\r\n\r\n                    // if " +
                    "m has no other incoming edges then\r\n                    if (edges.All(me => me.I" +
                    "tem2.Equals(m) == false))\r\n                    {\r\n                        // ins" +
                    "ert m into S\r\n                        S.Add(m);\r\n                    }\r\n        " +
                    "        }\r\n            }\r\n\r\n            // if graph has edges then\r\n            " +
                    "if (edges.Any())\r\n            {\r\n                // return error (graph has at l" +
                    "east one cycle)\r\n                return null;\r\n            }\r\n            else\r\n" +
                    "            {\r\n                // return L (a topologically sorted order)\r\n     " +
                    "           return L;\r\n            }\r\n        }\r\n\r\n        public List<KeyValuePa" +
                    "ir<string, string>> DependencyGraph { get; }\r\n\r\n        private string CreateDep" +
                    "endencyNodeName(string vmId, params string[] propertyNames) => $\"{vmId}.{string." +
                    "Join(\".\", propertyNames)}\";\r\n\r\n        private string GetViewModelIdFromDependen" +
                    "cyPath(string path) => string.Join(\".\", path.Split(\".\").SkipLast(1));\r\n\r\n       " +
                    " private string GetDependencyNameFromDependencyPath(string path) => path.Split(\'" +
                    ".\').Last();\r\n    }\r\n}\r\n");
            return this.GenerationEnvironment.ToString();
        }
    }
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    internal class DependencyGraphManagerTemplateBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}