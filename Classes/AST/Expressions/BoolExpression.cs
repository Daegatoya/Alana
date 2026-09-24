using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class BoolExpression : Expression
    {
        public bool value;

        public BoolExpression(bool value)
        {
            this.value = value;
        }
    }
}
