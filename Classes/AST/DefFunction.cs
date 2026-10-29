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
        public Visibility? visibility;
        public string? ownermodule;
        public VarType? return_type;
        public string? returnClassName;
        public bool returnIsArray;
        public bool isOverride;
        public bool isAbstract;

        public DefFunction(Visibility? visibility, string name, List<Param>? parameters = null, List<Statement>? body = null, VarType? return_type = null, string? returnClassName = null, bool returnIsArray = false, bool isOverride = false, bool isAbstract = false)
        {
            this.name = name;
            this.parameters = parameters;
            this.body = body;
            this.visibility = visibility;
            this.return_type = return_type;
            this.returnClassName = returnClassName;
            this.returnIsArray = returnIsArray;
            this.isOverride = isOverride;
            this.isAbstract = isAbstract;
        }
    }
}
