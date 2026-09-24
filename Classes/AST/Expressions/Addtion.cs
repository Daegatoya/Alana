using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class Addition : Expression
    {
        public Expression left;
        public Expression right;

        public Addition(Expression left, Expression right)
        {
            this.left = left;
            this.right = right;
        }
    }
}
