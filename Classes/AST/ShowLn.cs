using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class ShowLn : Statement
    {
        public Expression? expression;

        public ShowLn(Expression? expression = null)
        {
            this.expression = expression;
        }
    }
}
