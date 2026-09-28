using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class ParseChar : Expression
    {
        public Expression expression;

        public ParseChar(Expression expression)
        {
            this.expression = expression;
        }
    }
}
