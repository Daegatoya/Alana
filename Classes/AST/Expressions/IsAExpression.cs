using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class IsAExpression : Expression
    {
        public Expression value;
        public string className;

        public IsAExpression(Expression value, string className)
        {
            this.value = value;
            this.className = className;
        }
    }
}
