using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class MethodCall : Expression
    {
        public string objectName;
        public string methodName;
        public List<Expression> arguments;

        public MethodCall(string objectName, string methodName, List<Expression> arguments)
        {
            this.objectName = objectName;
            this.methodName = methodName;
            this.arguments = arguments;
        }
    }
}
