using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST.Expressions
{
    public abstract class Expression
    {
        public int line, col;
        public string? source = string.Empty;

        public Expression()
        {

        }
    }
}
