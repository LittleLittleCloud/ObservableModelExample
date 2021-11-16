using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObservableModelExample.Obseravable
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class DependsOnAttribute: Attribute
    {
        public DependsOnAttribute(params string[] properties)
        {
            this.Properties = properties;
        }

        public string[] Properties { get; }
    }
}
