using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class NotExpression : Expression
    {
        public Expression expression;

        public NotExpression(Expression expression)
        {
            this.expression = expression;
        }
    }
}
