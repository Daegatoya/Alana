using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class ArrayAccessExpression : Expression
    {
        public string name;
        public Expression index;

        public ArrayAccessExpression(string name, Expression index)
        {
            this.name = name;
            this.index = index;
        }
    }
}
