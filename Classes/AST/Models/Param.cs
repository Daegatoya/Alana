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
        public VarType type;
        public object? value = null;
        public bool isArray = false;

        public Param(string name, VarType type, bool isArray)
        {
            this.name = name;
            this.type = type;
            this.isArray = isArray;
        }
    }
}
