using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class PopStatement : Statement
    {
        public Expression target;

        public PopStatement(Expression target)
        {
            this.target = target;
        }
    }
}
