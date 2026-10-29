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
        internal AlanaError? lastCaught;

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

        internal AlanaError UserThrow(object value)
        {
            return new AlanaError(MessageFromThrown(value), line, col, source);
        }

        internal object CaughtOrCurrent(string kind)
        {
            if (lastCaught != null)
            {
                if (kind == "message")
                {
                    return lastCaught.Message;
                }

                if (kind == "line")
                {
                    return lastCaught.line;
                }

                if (kind == "col")
                {
                    return lastCaught.col;
                }

                return lastCaught.source;
            }

            if (kind == "message")
            {
                return "";
            }

            if (kind == "line")
            {
                return line;
            }

            if (kind == "col")
            {
                return col;
            }

            return source;
        }

        internal string MessageFromThrown(object value)
        {
            if (value is string message)
            {
                return message;
            }

            if (value is ClassInstance instance)
            {
                if (TryFindPrimitiveField(instance.definition, "message", out DefineVar field, out DefClass owner))
                {
                    if (field.type == VarType.STR && !field.isArray)
                    {
                        RunTimeVariable stored = instance.fields.GetLocal("message");

                        if (stored.isInitialized && stored.value is string fieldMessage)
                        {
                            return fieldMessage;
                        }
                    }
                }
            }

            return value.ToString()!;
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

        internal bool IsSubclassOrSame(string actualName, string expectedName)
        {
            DefClass current = ResolveClass(actualName);

            while (true)
            {
                if (current.name == expectedName)
                {
                    return true;
                }

                if (current.parentName == null)
                {
                    return false;
                }

                current = ResolveClass(current.parentName);
            }
        }

        internal void CheckObject(object value, string className, bool isArray)
        {
            if (isArray)
            {
                if (value is not List<object> values)
                {
                    throw Error($"Type error: expected an array of {className}");
                }

                foreach (object element in values)
                {
                    CheckObject(element, className, false);
                }

                return;
            }

            if (value is not ClassInstance instance)
            {
                throw Error($"Type error: expected object of class {className}");
            }

            if (!IsSubclassOrSame(instance.definition.name, className))
            {
                throw Error($"Type error: expected {className} but received {instance.definition.name}");
            }
        }

        internal void BindParameter(Scope scope, Param parameter, object value)
        {
            if (parameter.className != null)
            {
                CheckObject(value, parameter.className, parameter.isArray);

                if (parameter.isArray)
                {
                    scope.DefineObjectArray(parameter.name, (List<object>)value, Visibility.PRIVATE, null, parameter.className);
                }
                else
                {
                    scope.DefineObject(parameter.name, (ClassInstance)value, Visibility.PRIVATE, null, parameter.className);
                }
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

        internal bool TryFindMethod(DefClass start, string name, out DefFunction method, out DefClass owner)
        {
            DefClass? current = start;

            while (current != null)
            {
                DefFunction? found = current.methods.FirstOrDefault(m => m.name == name);

                if (found != null)
                {
                    method = found;
                    owner = current;
                    return true;
                }

                if (current.parentName == null)
                {
                    break;
                }

                current = ResolveClass(current.parentName);
            }

            method = null!;
            owner = null!;
            return false;
        }

        internal bool TryFindPrimitiveField(DefClass start, string name, out DefineVar field, out DefClass owner)
        {
            DefClass? current = start;

            while (current != null)
            {
                DefineVar? found = current.fields.FirstOrDefault(f => f.name == name);

                if (found != null)
                {
                    field = found;
                    owner = current;
                    return true;
                }

                if (current.parentName == null)
                {
                    break;
                }

                current = ResolveClass(current.parentName);
            }

            field = null!;
            owner = null!;
            return false;
        }

        internal bool TryFindObjectField(DefClass start, string name, out DefineObject field, out DefClass owner)
        {
            DefClass? current = start;

            while (current != null)
            {
                DefineObject? found = current.objectFields.FirstOrDefault(f => f.name == name);

                if (found != null)
                {
                    field = found;
                    owner = current;
                    return true;
                }

                if (current.parentName == null)
                {
                    break;
                }

                current = ResolveClass(current.parentName);
            }

            field = null!;
            owner = null!;
            return false;
        }

        private bool SignaturesMatch(DefFunction left, DefFunction right)
        {
            if (left.parameters!.Count != right.parameters!.Count)
            {
                return false;
            }

            if (left.return_type != right.return_type)
            {
                return false;
            }

            if (left.returnClassName != right.returnClassName)
            {
                return false;
            }

            if (left.returnIsArray != right.returnIsArray)
            {
                return false;
            }

            for (int i = 0; i < left.parameters.Count; i++)
            {
                Param a = left.parameters[i];
                Param b = right.parameters[i];

                if (a.type != b.type)
                {
                    return false;
                }

                if (a.className != b.className)
                {
                    return false;
                }

                if (a.isArray != b.isArray)
                {
                    return false;
                }
            }

            return true;
        }

        private void InvokeConstructor(ClassInstance instance, DefClass owner, DefConstructor constructor, List<Expression> arguments, Scope callerScope)
        {
            if (arguments.Count != constructor.parameters.Count)
            {
                throw Error($"Constructor for {owner.name} expected " + $"{constructor.parameters.Count} arguments but received {arguments.Count}");
            }

            Scope scope = instance.fields.CreateChild();
            scope.currentInstance = instance;
            scope.currentClass = owner;

            for (int i = 0; i < constructor.parameters.Count; i++)
            {
                Param parameter = constructor.parameters[i];
                Expression argument = arguments[i];
                object value = evaluator.Evaluate(argument, callerScope);

                BindParameter(scope, parameter, value);
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
                        throw Error($"Constructor for {owner.name} cannot return a value");
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

        internal object? InvokeParentConstructor(List<Expression> arguments, Scope callerScope)
        {
            ClassInstance? instance = callerScope.currentInstance;
            DefClass? currentClass = callerScope.currentClass;

            if (instance == null || currentClass == null)
            {
                throw Error("parent can only be used inside a class");
            }

            if (currentClass.parentName == null)
            {
                throw Error($"Class {currentClass.name} has no parent");
            }

            DefClass parent = ResolveClass(currentClass.parentName);

            if (parent.constructor != null)
            {
                InvokeConstructor(instance, parent, parent.constructor, arguments, callerScope);
            }
            else if (arguments.Count > 0)
            {
                throw Error($"Class {parent.name} has no constructor");
            }

            return null;
        }

        internal object? InvokeMethod(MethodCall call, Scope callerScope)
        {
            DefClass? startClass = null;
            ClassInstance? instance = null;

            if (call.target is VarExpression parentTarget && parentTarget.name == "parent")
            {
                instance = callerScope.currentInstance;
                DefClass? currentClass = callerScope.currentClass;

                if (instance == null || currentClass == null)
                {
                    throw Error("parent can only be used inside a class");
                }

                if (currentClass.parentName == null)
                {
                    throw Error($"Class {currentClass.name} has no parent");
                }

                startClass = ResolveClass(currentClass.parentName);
            }
            else
            {
                object target = evaluator.Evaluate(call.target, callerScope);

                if (target is not ClassInstance evaluated)
                {
                    throw Error($"Cannot call method {call.methodName} on a non-object value");
                }

                instance = evaluated;
                startClass = instance.definition;
            }

            if (!TryFindMethod(startClass, call.methodName, out DefFunction method, out DefClass owner))
            {
                throw Error($"Class {startClass.name} has no method {call.methodName}");
            }

            if (method.isAbstract)
            {
                throw Error($"Cannot call abstract method {call.methodName}");
            }

            if (method.visibility == Visibility.PRIVATE)
            {
                if (callerScope.currentClass == null || callerScope.currentClass != owner)
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
            scope.currentClass = owner;

            for (int i = 0; i < method.parameters.Count; i++)
            {
                Param parameter = method.parameters[i];
                Expression argument = call.arguments[i];

                object value = evaluator.Evaluate(argument, callerScope);

                BindParameter(scope, parameter, value);
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
                    if (result.Value is null)
                    {
                        throw Error($"Method {method.name} expected return type " + $"{method.returnClassName}" + $"{(method.returnIsArray ? "[]" : "")}");
                    }

                    CheckObject(result.Value, method.returnClassName, method.returnIsArray);
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

        private void InitClassMembers(DefClass definition, ClassInstance instance)
        {
            if (definition.parentName != null)
            {
                InitClassMembers(ResolveClass(definition.parentName), instance);
            }

            instance.fields.currentClass = definition;
            instance.fields.currentInstance = instance;

            foreach (DefineVar field in definition.fields)
            {
                ExecResult? result = executor.Execute(new List<Statement> { field }, instance.fields);

                if (result != null)
                {
                    throw FlowError(result.Flow);
                }
            }

            foreach (DefineObject field in definition.objectFields)
            {
                ExecResult? result = executor.Execute(new List<Statement> { field }, instance.fields);

                if (result != null)
                {
                    throw FlowError(result.Flow);
                }
            }
        }

        internal ClassInstance InstantiateClass(string className, List<Expression> arguments, Scope callerScope)
        {
            DefClass definition = ResolveClass(className);

            if (definition.isAbstract)
            {
                throw Error($"Cannot create an instance of abstract class {className}");
            }

            Scope fieldScope = globals.CreateChild();
            ClassInstance instance = new ClassInstance(definition, fieldScope);
            fieldScope.currentInstance = instance;
            fieldScope.currentClass = definition;

            if (definition.ownermodule != null)
            {
                moduleLoader.EnterModule(definition.ownermodule);
            }

            try
            {
                InitClassMembers(definition, instance);
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
                InvokeConstructor(instance, definition, definition.constructor, arguments, callerScope);
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

                if (method.isAbstract && !classDefined.isAbstract)
                {
                    throw Error($"Abstract method {method.name} can only be declared in an abstract class");
                }

                if (method.isOverride && classDefined.parentName == null)
                {
                    throw Error($"Method {method.name} uses override but class {classDefined.name} has no parent");
                }
            }

            if (classDefined.parentName != null)
            {
                if (classDefined.parentName == classDefined.name)
                {
                    throw Error($"Class {classDefined.name} cannot extend itself");
                }

                DefClass parent = ResolveClass(classDefined.parentName);

                foreach (DefFunction method in classDefined.methods)
                {
                    bool parentHasMethod = TryFindMethod(parent, method.name, out DefFunction parentMethod, out DefClass parentOwner);

                    if (method.isOverride)
                    {
                        if (!parentHasMethod)
                        {
                            throw Error($"Method {method.name} has no method to override");
                        }

                        if (parentMethod.visibility == Visibility.PRIVATE)
                        {
                            throw Error($"Method {method.name} cannot override a private method");
                        }

                        if (!SignaturesMatch(method, parentMethod))
                        {
                            throw Error($"Method {method.name} does not match the overridden method");
                        }
                    }
                    else if (parentHasMethod && parentMethod.visibility != Visibility.PRIVATE)
                    {
                        throw Error($"Method {method.name} hides a parent method and must use override");
                    }
                }

                if (!classDefined.isAbstract)
                {
                    DefClass? walk = parent;

                    while (walk != null)
                    {
                        foreach (DefFunction parentMethod in walk.methods)
                        {
                            if (!parentMethod.isAbstract)
                            {
                                continue;
                            }

                            if (!TryFindMethod(classDefined, parentMethod.name, out DefFunction resolved, out DefClass resolvedOwner))
                            {
                                throw Error($"Class {classDefined.name} does not override abstract method {parentMethod.name}");
                            }

                            if (resolved.isAbstract)
                            {
                                throw Error($"Class {classDefined.name} does not override abstract method {parentMethod.name}");
                            }
                        }

                        if (walk.parentName == null)
                        {
                            break;
                        }

                        walk = ResolveClass(walk.parentName);
                    }
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

                    BindParameter(scope, p, value);
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
                        if (result.Value is null)
                        {
                            throw Error($"Function {function.name} expected return type " + $"{function.returnClassName}" + $"{(function.returnIsArray ? "[]" : "")}");
                        }

                        CheckObject(result.Value, function.returnClassName, function.returnIsArray);
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
