using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class DefineObject : Statement
    {
        public Visibility visibility;
        public string className;
        public string name;
        public Expression value;
        public bool isArray;

        public DefineObject(Visibility visibility, string className, string name, Expression value, bool isArray = false)
        {
            this.visibility = visibility;
            this.className = className;
            this.name = name;
            this.value = value;
            this.isArray = isArray;
        }
    }
}
