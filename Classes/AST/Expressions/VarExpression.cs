using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class VarExpression : Expression
    {
        public string name = string.Empty;

        public VarExpression(string name) 
        {
            this.name = name;
        }
    }
}
