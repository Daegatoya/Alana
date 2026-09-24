using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classes.AST.Expressions;

namespace Classes.AST
{
    public class Show : Statement
    {
        public Expression expression;

        public Show(Expression expression)
        {
            this.expression = expression;
        }
    }
}
