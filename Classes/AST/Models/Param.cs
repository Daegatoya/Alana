using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classes.Lexer;

namespace Classes.AST.Models
{
    public class Param
    {
        public string name = string.Empty;
        public VarType? type;
        public object? value = null;
        public bool isArray = false;
        public string? className;

        public Param(string name, VarType type, bool isArray)
        {
            this.name = name;
            this.type = type;
            this.isArray = isArray;
            this.className = null;
        }

        public Param(string name, string className, bool isArray = false)
        {
            this.name = name;
            this.className = className;
            this.isArray = isArray;
            this.type = null;
        }
    }
}
