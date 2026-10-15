using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class PushStatement : Statement
    {
        public Expression target;
        public Expression value;

        public PushStatement(Expression target, Expression value)
        {
            this.target = target;
            this.value = value;
        }
    }
}
