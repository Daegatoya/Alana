using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class PopStatement : Statement
    {
        public string name;

        public PopStatement(string name)
        {
            this.name = name;
        }
    }
}
