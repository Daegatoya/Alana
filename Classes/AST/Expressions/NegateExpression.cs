using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class NegateExpression : Expression
    {
        public Expression expression;

        public NegateExpression(Expression expression)
        {
            this.expression = expression;
        }
    }
}
