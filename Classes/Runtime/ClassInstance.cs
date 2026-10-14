using Classes.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Runtime
{
    public class ClassInstance
    {
        public DefClass definition;
        public Scope fields;

        public ClassInstance(DefClass definition, Scope fields)
        {
            this.definition = definition;
            this.fields = fields;
        }
    }
}
