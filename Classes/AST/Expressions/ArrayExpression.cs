using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class ArrayExpression : Expression
    {
        public List<Expression> values = new();

        public ArrayExpression(List<Expression> values)
        {
            this.values = values;
        }
    }
}
