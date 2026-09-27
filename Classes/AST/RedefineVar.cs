using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class RedefineVar : Statement
    {
        public string name {  get; set; } = string.Empty;
        public Expression? value { get; set; }

        public RedefineVar(string name, Expression? value)
        {
            this.name = name;
            this.value = value;
        }
    }
}
