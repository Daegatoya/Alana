using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class RedefineArrElement : Statement
    {
        public string name;
        public Expression index;
        public Expression value;

        public RedefineArrElement(string name, Expression index, Expression value)
        {
            this.name = name;
            this.index = index;
            this.value = value;
        }
    }
}
