using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class ReturnStatement : Statement
    {
        public Expression? expression = null;

        public ReturnStatement(Expression? expression)
        {
            this.expression = expression;
        }
    }
}
