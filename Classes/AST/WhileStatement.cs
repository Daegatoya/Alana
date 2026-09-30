using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class WhileStatement : Statement
    {
        public Expression expression;
        public List<Statement> body;

        public WhileStatement(Expression expression, List<Statement> body)
        {
            this.expression = expression;
            this.body = body;
        }
    }
}
