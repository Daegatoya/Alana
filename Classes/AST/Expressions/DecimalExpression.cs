using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class DecimalExpression : Expression
    {
        public decimal value = 0;

        public DecimalExpression(decimal value)
        {
            this.value = value;
        }
    }
}
