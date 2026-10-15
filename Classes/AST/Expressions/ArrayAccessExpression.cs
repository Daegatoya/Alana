using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class ArrayAccessExpression : Expression
    {
        public Expression target;
        public Expression index;

        public ArrayAccessExpression(Expression target, Expression index)
        {
            this.target = target;
            this.index = index;
        }
    }
}
