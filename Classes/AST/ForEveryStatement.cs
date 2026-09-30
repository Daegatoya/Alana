using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class ForEveryStatement : Statement
    {
        public string name = string.Empty;
        public VarType type;
        public bool isArray = false;
        public string array = string.Empty;
        public List<Statement> body;

        public ForEveryStatement(string name, VarType type, bool isArray, string array, List<Statement> body)
        {
            this.name = name;
            this.type = type;
            this.isArray = isArray;
            this.array = array;
            this.body = body;
        }
    }
}
