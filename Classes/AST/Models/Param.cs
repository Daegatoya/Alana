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
        public VarType? type = null;
        public object? value = null;

        public Param(string name, VarType? type)
        {
            this.name = name;
            this.type = type;
        }
    }
}
