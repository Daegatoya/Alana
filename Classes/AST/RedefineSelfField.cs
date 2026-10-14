using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class RedefineSelfField : Statement
    {
        public string fieldName;
        public Expression value;

        public RedefineSelfField(string fieldName, Expression value)
        {
            this.fieldName = fieldName;
            this.value = value;
        }
    }
}
