using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class CharExpression : Expression
    {
        public char value;

        public CharExpression(char value)
        {
            this.value = value;
        }
    }
}
