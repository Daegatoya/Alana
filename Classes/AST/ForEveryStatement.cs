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

        public ForEveryStatement(string name, VarType type, bool isArray, Expression target, List<Statement> body)
        {
            this.name = name;
            this.type = type;
            this.isArray = isArray;
            this.target = target;
            this.body = body;
        }
    }
}
