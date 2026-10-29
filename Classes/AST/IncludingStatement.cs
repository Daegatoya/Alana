using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class IncludingStatement : Statement
    {
        public string? path;

        public IncludingStatement(string? path)
        {
            this.path = path;
        }
    }
}
