using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class ParseBool : Expression
    {
        public Expression expression;

        public ParseBool(Expression expression)
        {
            this.expression = expression;
        }
    }
}
