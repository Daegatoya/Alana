using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public enum VarType
    {
        NUM,
        STR,
    }
    public class DefineVar : Statement
    {
        public string name = string.Empty;
        public VarType type { get; private set; }
        public object value = string.Empty;

        public DefineVar(string name, VarType type, object value)
        {
            this.name = name;
            this.type = type;
            this.value = value;
        }
    }
}
