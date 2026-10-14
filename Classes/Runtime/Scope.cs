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
        private Dictionary<string, RunTimeObject> objects = new();

        private Scope? parent;
        private Interpreter runtime;

        public ClassInstance? currentInstance;

        public Scope(Interpreter runtime, Scope? parent = null)
        {
            this.runtime = runtime;
            this.parent = parent;
        }

        public Scope CreateChild()
        {
            Scope child = new Scope(runtime, this);

            child.currentInstance = currentInstance;

            return child;
        }

        public bool IsGlobal()
        {
            return parent == null;
        }

        public void Define(string name, RunTimeVariable variable)
        {
            if (objects.ContainsKey(name))
            {
                throw runtime.Error($"An object named {name} is already defined");
            }
            if (variable.visibility == Visibility.PRIVATE &&
                variable.ownermodule != null)
            {
                string key = variable.ownermodule + "::" + name;

                if (variables.ContainsKey(key))
                {
                    throw runtime.Error($"Variable {name} is already defined");
                }

                if (variables.ContainsKey(name))
                {
                    RunTimeVariable existing = variables[name];

                    if (existing.ownermodule == variable.ownermodule)
                    {
                        throw runtime.Error($"Variable {name} is already defined");
                    }
                }

                variables.Add(key, variable);
            }
            else
            {
                if (variables.ContainsKey(name))
                {
                    throw runtime.Error($"Variable {name} is already defined");
                }

                if (variable.ownermodule != null)
                {
                    string privateKey =
                        variable.ownermodule + "::" + name;

                    if (variables.ContainsKey(privateKey))
                    {
                        throw runtime.Error($"Variable {name} is already defined");
                    }
                }

                variables.Add(name, variable);
            }
        }

        public void DefineObject(string name, ClassInstance instance, Visibility visibility, string? ownermodule)
        {
            string key = name;

            if (visibility == Visibility.PRIVATE && ownermodule != null)
            {
                key = ownermodule + "::" + name;
            }

            if (objects.ContainsKey(key))
            {
                throw runtime.Error($"Object {name} is already defined");
            }

            if (variables.ContainsKey(key))
            {
                throw runtime.Error($"A variable named {name} is already defined");
            }

            objects.Add(key, new RunTimeObject(instance, visibility, ownermodule));
        }

        public RunTimeVariable? Find(string name)
        {
            string current = runtime.moduleLoader.CurrentFile;
            string privateKey = current + "::" + name;

            Scope? scope = this;

            while (scope != null)
            {
                if (scope.variables.ContainsKey(privateKey))
                {
                    return scope.variables[privateKey];
                }

                if (scope.variables.ContainsKey(name))
                {
                    return scope.variables[name];
                }

                scope = scope.parent;
            }

            return null;
        }

        public ClassInstance? FindObject(string name)
        {
            string current = runtime.moduleLoader.CurrentFile;
            string privateKey = current + "::" + name;

            Scope? scope = this;

            while (scope != null)
            {
                if (scope.objects.TryGetValue(privateKey, out RunTimeObject? privateObject))
                {
                    return privateObject.instance;
                }

                if (scope.objects.TryGetValue(name, out RunTimeObject? publicObject))
                {
                    return publicObject.instance;
                }

                scope = scope.parent;
            }

            return null;
        }

        public RunTimeVariable? FindLocal(string name)
        {
            if (variables.TryGetValue(name, out RunTimeVariable? variable))
            {
                return variable;
            }

            return null;
        }

        public RunTimeVariable GetLocal(string name)
        {
            RunTimeVariable? variable = FindLocal(name);

            if (variable == null)
            {
                throw runtime.Error($"Field {name} can't be found");
            }

            return variable;
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

        public ClassInstance GetObject(string name)
        {
            ClassInstance? instance = FindObject(name);

            if (instance == null)
            {
                throw runtime.Error($"Object {name} can't be found");
            }

            return instance;
        }
    }
}
