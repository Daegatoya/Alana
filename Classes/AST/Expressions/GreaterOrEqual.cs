using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class GreaterOrEqual : Expression
    {
        public Expression left;
        public Expression right;

        public GreaterOrEqual(Expression left, Expression right)
        {
            this.left = left;
            this.right = right;
        }
    }
}
