using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObservableModelExample.Obseravable
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class UpdateAttribute: Attribute
    {
        public UpdateAttribute(string property)
        {
            this.Property = property;
        }

        public string Property { get; }
    }
}
