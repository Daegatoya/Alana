using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Runtime
{
    public class RunTimeObject
    {
        public ClassInstance instance;
        public Visibility visibility;
        public string? ownermodule;

        public RunTimeObject(ClassInstance instance, Visibility visibility, string? ownermodule)
        {
            this.instance = instance;
            this.visibility = visibility;
            this.ownermodule = ownermodule;
        }
    }
}
