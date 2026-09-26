using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class ElseIfStatement : Statement
    {
        public Expression condition;
        public List<Statement> body;

        public ElseIfStatement(Expression condition, List<Statement> body)
        {
            this.condition = condition;
            this.body = body;
        }
    }
}
