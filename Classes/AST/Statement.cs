using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public abstract class Statement
    {
        public int line, col;
        public string? source = string.Empty;

        public Statement()
        {

        }
    }
}
