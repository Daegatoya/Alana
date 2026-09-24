using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class StringExpression : Expression
    {
        public string value = string.Empty;

        public StringExpression(string value)
        {
            this.value = value;
        }
    }
}
