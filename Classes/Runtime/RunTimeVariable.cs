using Classes.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Runtime
{
    public class RunTimeVariable
    {
        public VarType type;
        public object? value = null;
        public bool isInitialized = false;
        public bool isArray = false;

        public RunTimeVariable(VarType type, object? value, bool isInitialized, bool isArray)
        {
            this.type = type;
            this.value = value;
            this.isInitialized = isInitialized;
            this.isArray = isArray;
        }
    }
}
