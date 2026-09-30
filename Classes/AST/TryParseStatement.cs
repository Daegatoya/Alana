using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class TryParseStatement : Statement
    {
        public Expression value;
        public string target;
        public string success;
        public VarType type;

        public TryParseStatement(Expression value, string target, string success, VarType type)
        {
            this.value = value;
            this.target = target;
            this.success = success;
            this.type = type;
        }
    }
}
