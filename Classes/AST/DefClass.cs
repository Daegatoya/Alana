using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.AST
{
    public class DefClass : Statement
    {
        public Visibility? visibility;
        public string name;
        public string? ownermodule;
        public List<DefineVar> fields;
        public List<DefineObject> objectFields;
        public List<DefFunction> methods;
        public DefConstructor? constructor;

        public DefClass(Visibility visibility, string name, List<DefineVar> fields, List<DefineObject> objectFields, List<DefFunction> methods, DefConstructor? constructor)
        {
            this.visibility = visibility;
            this.name = name;
            this.fields = fields;
            this.objectFields = objectFields;
            this.methods = methods;
            this.constructor = constructor;
        }
    }
}
