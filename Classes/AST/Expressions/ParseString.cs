using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class ParseString : Expression
    {
        public Expression expression;

        public ParseString(Expression expression)
        {
            this.expression = expression;
        }
    }
}
