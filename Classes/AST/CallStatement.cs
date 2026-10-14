using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class CallStatement : Statement
    {
        public Expression call;

        public CallStatement(Expression call)
        {
            this.call = call;
        }
    }
}
