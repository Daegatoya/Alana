using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class NewObject : Expression
    {
        public string className;
        public List<Expression> arguments;

        public NewObject(string className, List<Expression> arguments)
        {
            this.className = className;
            this.arguments = arguments;
        }
    }
}
