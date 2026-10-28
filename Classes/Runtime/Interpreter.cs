using Classes.AST;
using Classes.AST.Expressions;
using Classes.AST.Models;
using Classes.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Runtime
{
    public class Interpreter
    {
        private Dictionary<string, DefFunction> functions = new();
        private Dictionary<string, DefClass> classes = new();
        private Scope globals;
        private ExpressionEvaluator evaluator;
        private StatementExecutor executor;
        private NativeFunctions nativeFunctions;
        public ModuleLoader moduleLoader;

        internal int line, col;
        internal string source = string.Empty;

        public Interpreter(string entryFile)
        {
            evaluator = new ExpressionEvaluator(this);
            executor = new StatementExecutor(this, evaluator);
            globals = new Scope(this);
            moduleLoader = new ModuleLoader(this, entryFile);
            nativeFunctions = new NativeFunctions(this, evaluator);
        }

        internal AlanaError Error(string message)
        {
            return new AlanaError(message, line, col, source);
        }

        internal void SetPos(int line, int col, string? source)
        {
            this.line = line;
            this.col = col;
            this.source = source ?? "";
        }

        public void Execute(List<Statement> statements)
        {
            ExecResult? result = executor.Execute(statements, globals);

            if (result != null)
            {
                throw FlowError(result.Flow);
            }
        }

        internal DefClass ResolveClass(string name)
        {
            string privateKey = moduleLoader.CurrentFile + "::" + name;

            if (classes.TryGetValue(privateKey, out DefClass? privateClass))
            {
                return privateClass;
            }

            if (classes.TryGetValue(name, out DefClass? publicClass))
            {
                return publicClass;
            }

            foreach (DefClass c in classes.Values)
            {
                if (c.name == name && c.visibility == Visibility.PRIVATE)
                {
                    throw Error($"Class {name} is private");
                }
            }

            throw Error($"Unknown class {name}");
        }

        private void InvokeConstructor(ClassInstance instance, DefConstructor constructor, List<Expression> arguments, Scope callerScope)
        {
            if (arguments.Count != constructor.parameters.Count)
            {
                throw Error($"Constructor for {instance.definition.name} expected " + $"{constructor.parameters.Count} arguments but received {arguments.Count}");
            }

            Scope scope = instance.fields.CreateChild();
            scope.currentInstance = instance;

            for (int i = 0; i < constructor.parameters.Count; i++)
            {
                Param parameter = constructor.parameters[i];
                Expression argument = arguments[i];
                object value = evaluator.Evaluate(argument, callerScope);

                if (parameter.className != null)
                {
                    if (value is not ClassInstance objectValue)
                    {
                        throw Error($"Parameter {parameter.name} expected object of class {parameter.className}");
                    }

                    if (objectValue.definition.name != parameter.className)
                    {
                        throw Error($"Parameter {parameter.name} expected {parameter.className} " + $"but received {objectValue.definition.name}");
                    }

                    scope.DefineObject(parameter.name, objectValue, Visibility.PRIVATE, null);
                }
                else
                {
                    if (!TypeChecker.CheckType(this, value, parameter.type!.Value, parameter.isArray))
                    {
                        throw Error($"Parsed argument {value} has the wrong type for parameter " + $"{parameter.name} of type {parameter.type}");
                    }

                    scope.Define(parameter.name, new RunTimeVariable(parameter.type.Value, value, true, parameter.isArray));
                }
            }

            if (instance.definition.ownermodule != null)
            {
                moduleLoader.EnterModule(instance.definition.ownermodule);
            }

            try
            {
                ExecResult? result = executor.Execute(constructor.body, scope);

                    if (result != null)
                    {
                        if (result.Flow == ExecFlow.Return)
                        {
                        throw Error($"Constructor for {instance.definition.name} cannot return a value");
                    }

                    throw FlowError(result.Flow);
                }
            }
            finally
            {
                if (instance.definition.ownermodule != null)
                {
                    moduleLoader.ExitModule();
                }
            }
        }

        internal object? InvokeMethod(MethodCall call, Scope callerScope)
        {
            object target = evaluator.Evaluate(call.target, callerScope);

            if (target is not ClassInstance instance)
            {
                throw Error($"Cannot call method {call.methodName} on a non-object value");
            }

            DefFunction? method = instance.definition.methods.FirstOrDefault(m => m.name == call.methodName);

            if (method == null)
            {
                throw Error($"Class {instance.definition.name} has no method {call.methodName}");
            }

            if (method.visibility == Visibility.PRIVATE)
            {
                ClassInstance? callerInstance = callerScope.currentInstance;

                if (callerInstance == null || callerInstance.definition != instance.definition)
                {
                    throw Error($"Method {call.methodName} is private");
                }
            }

            if (call.arguments.Count != method.parameters!.Count)
            {
                throw Error($"Method {call.methodName} expected " + $"{method.parameters.Count} arguments but received " + $"{call.arguments.Count}");
            }

            Scope scope = instance.fields.CreateChild();

            scope.currentInstance = instance;

            for (int i = 0; i < method.parameters.Count; i++)
            {
                Param parameter = method.parameters[i];
                Expression argument = call.arguments[i];

                object value = evaluator.Evaluate(argument, callerScope);

                if (parameter.className != null)
                {
                    if (value is not ClassInstance objectValue)
                    {
                        throw Error($"Parameter {parameter.name} expected object " + $"of class {parameter.className}");
                    }

                    if (objectValue.definition.name != parameter.className)
                    {
                        throw Error($"Parameter {parameter.name} expected " + $"{parameter.className} but received " + $"{objectValue.definition.name}");
                    }

                    scope.DefineObject(parameter.name, objectValue, Visibility.PRIVATE, null);
                }
                else
                {
                    if (!TypeChecker.CheckType(this, value, parameter.type!.Value, parameter.isArray))
                    {
                        throw Error($"Parsed argument {value} has the wrong type " + $"for parameter {parameter.name} of type {parameter.type}");
                    }

                    scope.Define(parameter.name, new RunTimeVariable(parameter.type.Value, value, true, parameter.isArray));
                }
            }

            if (instance.definition.ownermodule != null)
            {
                moduleLoader.EnterModule(instance.definition.ownermodule);
            }

            ExecResult? result;

            try
            {
                result = executor.Execute(method.body, scope);
            }
            finally
            {
                if (instance.definition.ownermodule != null)
                {
                    moduleLoader.ExitModule();
                }
            }

            if (result == null)
            {
                if (method.return_type is not null || method.returnClassName is not null)
                {
                    string expected = method.returnClassName ?? method.return_type.ToString()!;

                    throw Error($"Method {method.name} expected return type " + $"{expected} but returned nothing");
                }

                return null;
            }

            if (result.Flow == ExecFlow.Return)
            {
                if (method.returnClassName != null)
                {
                    if (result.Value is not ClassInstance objectValue)
                    {
                        throw Error($"Method {method.name} expected return type " + $"{method.returnClassName}");
                    }

                    if (objectValue.definition.name != method.returnClassName)
                    {
                        throw Error($"Method {method.name} expected return type " + $"{method.returnClassName} but returned " + $"{objectValue.definition.name}");
                    }
                }
                else if (method.return_type is not null)
                {
                    if (result.Value is null)
                    {
                        throw Error($"Method {method.name} expected return type " + $"{method.return_type}" + $"{(method.returnIsArray ? "[]" : "")}");
                    }

                    bool correctType = TypeChecker.CheckType(this, result.Value, method.return_type, method.returnIsArray);

                    if (!correctType)
                    {
                        throw Error($"Method {method.name} expected return type " + $"{method.return_type}" + $"{(method.returnIsArray ? "[]" : "")}");
                    }
                }

                return result.Value;
            }

            throw FlowError(result.Flow);
        }

        internal ClassInstance InstantiateClass(string className, List<Expression> arguments, Scope callerScope)
        {
            DefClass definition = ResolveClass(className);
            Scope fieldScope = globals.CreateChild();
            ClassInstance instance = new ClassInstance(definition, fieldScope);
            fieldScope.currentInstance = instance;

            if (definition.ownermodule != null)
            {
                moduleLoader.EnterModule(definition.ownermodule);
            }

            try
            {
                foreach (DefineVar field in definition.fields)
                {
                    ExecResult? result = executor.Execute(new List<Statement> { field }, fieldScope);

                    if (result != null)
                    {
                        throw FlowError(result.Flow);
                    }
                }
                foreach (DefineObject field in definition.objectFields)
                {
                    object value = evaluator.Evaluate(field.value, instance.fields);

                    if (value is not ClassInstance objectValue)
                    {
                        throw Error($"Expected object of class {field.className}");
                    }

                    if (objectValue.definition.name != field.className)
                    {
                        throw Error($"Expected object of class {field.className} " + $"but received {objectValue.definition.name}");
                    }

                    instance.fields.DefineObject(field.name, objectValue, field.visibility, definition.ownermodule);
                }
            }
            finally
            {
                if (definition.ownermodule != null)
                {
                    moduleLoader.ExitModule();
                }
            }

            if (definition.constructor != null)
            {
                InvokeConstructor(instance, definition.constructor, arguments, callerScope);
            }
            else if (arguments.Count > 0)
            {
                throw Error($"Class {className} has no constructor");
            }

            return instance;
        }

        internal void DefineClass(DefClass classDefined)
        {
            classDefined.ownermodule = moduleLoader.CurrentFile;

            if (classDefined.visibility == null)
            {
                classDefined.visibility = Visibility.PUBLIC;
            }

            foreach (DefFunction method in classDefined.methods)
            {
                method.ownermodule = classDefined.ownermodule;

                if (method.visibility == null)
                {
                    method.visibility = Visibility.PRIVATE;
                }
            }

            string key = classDefined.name;

            if (classDefined.visibility == Visibility.PRIVATE && classDefined.ownermodule != null)
            {
                key = classDefined.ownermodule + "::" + classDefined.name;
            }

            if (classes.ContainsKey(key))
            {
                throw Error($"Class {classDefined.name} is already defined");
            }

            if (classDefined.visibility == Visibility.PRIVATE && classDefined.ownermodule != null)
            {
                if (classes.ContainsKey(classDefined.name))
                {
                    DefClass existing = classes[classDefined.name];

                    if (existing.ownermodule == classDefined.ownermodule)
                    {
                        throw Error($"Class {classDefined.name} is already defined");
                    }
                }
            }
            else if (classDefined.ownermodule != null)
            {
                string privateKey = classDefined.ownermodule + "::" + classDefined.name;

                if (classes.ContainsKey(privateKey))
                {
                    throw Error($"Class {classDefined.name} is already defined");
                }
            }

            classes.Add(key, classDefined);
        }

        internal void DefineFunction(DefFunction function)
        {
            function.ownermodule = moduleLoader.CurrentFile;

            if (function.visibility == null)
            {
                function.visibility = Visibility.PUBLIC;
            }

            string key = function.name;
            if (function.visibility == Visibility.PRIVATE && function.ownermodule != null)
            {
                key = function.ownermodule + "::" + function.name;
            }

            if (functions.ContainsKey(key))
            {
                throw Error($"Function {function.name} is already defined");
            }

            if (function.visibility == Visibility.PRIVATE && function.ownermodule != null)
            {
                if (functions.ContainsKey(function.name))
                {
                    DefFunction existing = functions[function.name];
                    if (existing.ownermodule == function.ownermodule)
                    {
                        throw Error($"Function {function.name} is already defined");
                    }
                }
            }
            else if (function.ownermodule != null)
            {
                string privateKey = function.ownermodule + "::" + function.name;
                if (functions.ContainsKey(privateKey))
                {
                    throw Error($"Function {function.name} is already defined");
                }
            }

            functions.Add(key, function);
        }

        internal object? Invoke(FuncCall call, Scope callerScope)
        {
            if (nativeFunctions.Exists(call.name))
            {
                return nativeFunctions.Invoke(call.name, call.arguments, callerScope);
            }
            string privateKey = moduleLoader.CurrentFile + "::" + call.name;
            DefFunction? function = null;

            if (functions.ContainsKey(privateKey))
            {
                function = functions[privateKey];
            }
            else if (functions.TryGetValue(call.name, out DefFunction? publicFunction))
            {
                function = publicFunction;
            }

            if (function != null)
            {
                Scope scope = globals.CreateChild();

                if (call.arguments.Count != function.parameters!.Count)
                {
                    throw Error($"Function {function.name} expected " + $"{function.parameters.Count} arguments but received " + $"{call.arguments.Count}");
                }

                for (int i = 0; i < function.parameters!.Count; i++)
                {
                    Param p = function.parameters[i];
                    Expression arg = call.arguments[i];
                    object value = evaluator.Evaluate(arg, callerScope);

                    if (p.className != null)
                    {
                        if (value is not ClassInstance objectValue)
                        {
                            throw Error($"Parameter {p.name} expected object of class {p.className}");
                        }

                        if (objectValue.definition.name != p.className)
                        {
                            throw Error($"Parameter {p.name} expected {p.className} " + $"but received {objectValue.definition.name}");
                        }

                        scope.DefineObject(p.name, objectValue, Visibility.PRIVATE, null);
                    }
                    else
                    {
                        if (!TypeChecker.CheckType(this, value, p.type!.Value, p.isArray))
                        {
                            throw Error($"Parsed argument {value} has the wrong type " + $"for parameter {p.name} of type {p.type}");
                        }

                        scope.Define(p.name, new RunTimeVariable(p.type.Value, value, true, p.isArray));
                    }
                }

                if (function.ownermodule != null)
                {
                    moduleLoader.EnterModule(function.ownermodule);
                }

                ExecResult? result;

                try
                {
                    result = executor.Execute(function.body, scope);
                }
                finally
                {
                    if (function.ownermodule != null)
                    {
                        moduleLoader.ExitModule();
                    }
                }

                if (result == null)
                {
                    if (function.return_type is not null || function.returnClassName is not null)
                    {
                        string expected = function.returnClassName ?? function.return_type.ToString()!;

                        throw Error($"Function {function.name} expected return type " + $"{expected} but returned nothing");
                    }

                    return null;
                }

                if (result.Flow == ExecFlow.Return)
                {
                    if (function.returnClassName != null)
                    {
                        if (result.Value is not ClassInstance objectValue)
                        {
                            throw Error($"Function {function.name} expected return type " + $"{function.returnClassName}");
                        }

                        if (objectValue.definition.name != function.returnClassName)
                        {
                            throw Error($"Function {function.name} expected return type " + $"{function.returnClassName} but returned " + $"{objectValue.definition.name}");
                        }
                    }
                    else if (function.return_type is not null)
                    {
                        if (result.Value is null)
                        {
                            throw Error($"Function {function.name} expected return type " + $"{function.return_type}" + $"{(function.returnIsArray ? "[]" : "")}");
                        }

                        bool isRightReturnType = TypeChecker.CheckType(this, result.Value, function.return_type, function.returnIsArray);

                        if (!isRightReturnType)
                        {
                            throw Error($"Function {function.name} expected return type " + $"{function.return_type}" + $"{(function.returnIsArray ? "[]" : "")}");
                        }
                    }

                    return result.Value;
                }

                throw FlowError(result.Flow);
            }
            else
            {
                foreach (DefFunction f in functions.Values)
                {
                    if (f.name == call.name && f.visibility == Visibility.PRIVATE)
                    {
                        throw Error($"Function {call.name} is private");
                    }
                }

                throw Error($"Unknown function {call.name}");
            }
        }

        private AlanaError FlowError(ExecFlow flow)
        {
            if (flow == ExecFlow.Return)
            {
                return Error("return can only be used inside a function");
            }

            if (flow == ExecFlow.Break)
            {
                return Error("break can only be used inside a loop");
            }

            return Error("continue can only be used inside a loop");
        }
    }
}
