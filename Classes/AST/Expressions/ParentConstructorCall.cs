using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class ParentConstructorCall : Expression
    {
        public List<Expression> arguments;

        public ParentConstructorCall(List<Expression> arguments)
        {
            this.arguments = arguments;
        }
    }
}
