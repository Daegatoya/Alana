using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public class Length : Expression
    {
        public string name;

        public Length(string name)
        {
            this.name = name;
        }
    }
}
