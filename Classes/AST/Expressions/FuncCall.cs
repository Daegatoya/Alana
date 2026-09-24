using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class FuncCall : Expression
    {
        public string name = string.Empty;
        public List<Expression> arguments = new();

        public FuncCall(string name, List<Expression> arguments)
        {
            this.name = name;
            this.arguments = arguments;
        }
    }
}
