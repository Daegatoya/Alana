using Classes.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Interpreter
{
    public class RunTimeVariable
    {
        public VarType type;
        public object? value = null;
        public bool isInitialized = false;

        public RunTimeVariable(VarType type, object? value, bool isInitialized)
        {
            this.type = type;
            this.value = value;
            this.isInitialized = isInitialized;
        }
    }
}
