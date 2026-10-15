using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class Length : Expression
    {
        public Expression target;

        public Length(Expression target)
        {
            this.target = target;
        }
    }
}
