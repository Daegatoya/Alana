using Classes.AST.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class DefFunction : Statement
    {
        public string name = string.Empty;
        public List<Param>? parameters = new();
        public List<Statement>? body = new();

        public DefFunction(string name, List<Param>? parameters = null, List<Statement>? body = null)
        {
            this.name = name;
            this.parameters = parameters;
            this.body = body;
        }
    }
}
