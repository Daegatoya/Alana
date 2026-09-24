using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class NumberExpression : Expression
    {
        public int value = 0;

        public NumberExpression(int value)
        {
            this.value = value;
        }
    }
}
