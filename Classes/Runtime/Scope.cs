using Classes.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Runtime
{
    public class Scope
    {
        private Dictionary<string, RunTimeVariable> variables = new();
        private Scope? parent;
        private Interpreter runtime;

        public Scope(Interpreter runtime, Scope? parent = null)
        {
            this.runtime = runtime;
            this.parent = parent;
        }

        public Scope CreateChild()
        {
            return new Scope(runtime, this);
        }

        public void Define(string name, RunTimeVariable variable)
        {
            if (variables.ContainsKey(name))
            {
                throw runtime.Error($"Variable {name} is already defined");
            }

            variables.Add(name, variable);
        }

        public RunTimeVariable? Find(string name)
        {
            Scope? scope = this;

            while (scope != null)
            {
                if (scope.variables.ContainsKey(name))
                {
                    return scope.variables[name];
                }

                scope = scope.parent;
            }

            return null;
        }

        public RunTimeVariable Get(string name)
        {
            RunTimeVariable? variable = Find(name);

            if (variable == null)
            {
                throw runtime.Error($"Variable {name} can't be found");
            }

            return variable;
        }
    }
}
