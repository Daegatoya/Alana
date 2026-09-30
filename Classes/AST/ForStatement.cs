using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class ForStatement : Statement
    {
        public DefineVar initialization;
        public Expression condition;
        public Statement step;
        public List<Statement> body;

        public ForStatement(DefineVar initialization, Expression condition, Statement step, List<Statement> body)
        {
            this.initialization = initialization;
            this.condition = condition;
            this.step = step;
            this.body = body;
        }
    }
}
