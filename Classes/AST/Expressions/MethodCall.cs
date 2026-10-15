using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class MethodCall : Expression
    {
        public Expression target;
        public string methodName;
        public List<Expression> arguments;

        public MethodCall(Expression target, string methodName, List<Expression> arguments)
        {
            this.target = target;
            this.methodName = methodName;
            this.arguments = arguments;
        }
    }
}
