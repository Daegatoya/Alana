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
        public string name;
        public Expression value;

        public PushStatement(string name, Expression value)
        {
            this.name = name;
            this.value = value;
        }
    }
}
