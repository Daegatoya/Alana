using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class TryStatement : Statement
    {
        public List<Statement> body;
        public string? catchName;
        public List<Statement>? catchBody;
        public List<Statement>? finallyBody;

        public TryStatement(List<Statement> body, string? catchName, List<Statement>? catchBody, List<Statement>? finallyBody)
        {
            this.body = body;
            this.catchName = catchName;
            this.catchBody = catchBody;
            this.finallyBody = finallyBody;
        }
    }
}
