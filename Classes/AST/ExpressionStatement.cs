using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class ExpressionStatement : Statement
    {
        public Expression expression;

        public ExpressionStatement(Expression expression)
        {
            this.expression = expression;
        }
    }
}
