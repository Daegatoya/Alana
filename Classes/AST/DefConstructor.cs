using Classes.AST.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class DefConstructor : Statement
    {
        public Visibility visibility;
        public List<Param> parameters;
        public List<Statement> body;

        public DefConstructor(Visibility visibility, List<Param> parameters, List<Statement> body)
        {
            this.visibility = visibility;
            this.parameters = parameters;
            this.body = body;
        }
    }
}
