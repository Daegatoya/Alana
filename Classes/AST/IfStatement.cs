using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class IfStatement : Statement
    {
        public Expression condition;
        public List<Statement> body;
        public List<Statement>? elseBody;
        public List<ElseIfStatement>? elseIfStatements;

        public IfStatement(Expression condition, List<Statement> body, List<Statement>? elseBody = null, List<ElseIfStatement>? elseIfStatements = null)
        {
            this.condition = condition;
            this.body = body;
            this.elseBody = elseBody;
            this.elseIfStatements = elseIfStatements;
        }
    }
}
