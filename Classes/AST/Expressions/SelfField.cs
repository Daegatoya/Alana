using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class SelfField : Expression
    {
        public string fieldName;

        public SelfField(string fieldName)
        {
            this.fieldName = fieldName;
        }
    }
}
