using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class ForEveryStatement : Statement
    {
        public VarType type;
        public string name;
        public bool isArray;
        public Expression target;
        public List<Statement> body;
        public string? className;

        public ForEveryStatement(string name, VarType type, bool isArray, Expression target, List<Statement> body, string? className = null)
        {
            this.name = name;
            this.type = type;
            this.isArray = isArray;
            this.target = target;
            this.body = body;
            this.className = className;
        }
    }
}
