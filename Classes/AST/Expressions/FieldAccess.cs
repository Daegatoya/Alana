using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class FieldAccess : Expression
    {
        public Expression target;
        public string fieldName;

        public FieldAccess(Expression target, string fieldName)
        {
            this.target = target;
            this.fieldName = fieldName;
        }
    }
}
