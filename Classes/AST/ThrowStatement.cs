using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class ThrowStatement : Statement
    {
        public Expression expression;

        public ThrowStatement(Expression expression)
        {
            this.expression = expression;
        }
    }
}
