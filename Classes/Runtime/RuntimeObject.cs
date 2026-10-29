using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Runtime
{
    public class RunTimeObject
    {
        public ClassInstance? instance;
        public List<object>? array;
        public bool isArray = false;
        public bool isInitialized = false;
        public string className = string.Empty;
        public Visibility visibility;
        public string? ownermodule;

        public RunTimeObject(ClassInstance instance, Visibility visibility, string? ownermodule, string className)
        {
            this.instance = instance;
            this.visibility = visibility;
            this.ownermodule = ownermodule;
            this.className = className;
            this.isInitialized = true;
        }

        public RunTimeObject(List<object> array, Visibility visibility, string? ownermodule, string className)
        {
            this.array = array;
            this.isArray = true;
            this.visibility = visibility;
            this.ownermodule = ownermodule;
            this.className = className;
            this.isInitialized = true;
        }

        public RunTimeObject(Visibility visibility, string? ownermodule, string className, bool isArray)
        {
            this.visibility = visibility;
            this.ownermodule = ownermodule;
            this.className = className;
            this.isArray = isArray;
            this.isInitialized = false;
        }
    }
}
