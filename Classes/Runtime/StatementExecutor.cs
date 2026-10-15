using Classes.AST;
using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Runtime
{
    internal class StatementExecutor
    {
        private Interpreter runtime;
        private ExpressionEvaluator expressions;

        public StatementExecutor(Interpreter runtime, ExpressionEvaluator expressions)
        {
            this.runtime = runtime;
            this.expressions = expressions;
        }

        public ExecResult? Execute(List<Statement>? statements, Scope scope)
        {
            if (statements is null) return null;

            foreach (Statement s in statements)
            {
                runtime.SetPos(s.line, s.col, s.source);

                if (s is ImportStatement import)
                {
                    runtime.moduleLoader.Load(import.path!);
                    continue;
                }

                if (s is DefineVar d)
                {
                    if (scope.IsGlobal())
                    {
                        d.ownermodule = runtime.moduleLoader.CurrentFile;

                        if (d.visibility == null)
                        {
                            d.visibility = Visibility.PUBLIC;
                        }
                    }

                    if (d.value is NewExpression)
                    {
                        if (scope.IsGlobal())
                        {
                            scope.Define(d.name, new RunTimeVariable(d.type, null, false, d.isArray, d.visibility, d.ownermodule));
                        }
                        else
                        {
                            scope.Define(d.name, new RunTimeVariable(d.type, null, false, d.isArray));
                        }
                    }
                    else
                    {
                        object value = expressions.Evaluate(d.value!, scope);
                        TypeChecker.CheckType(runtime, value, d.type, d.isArray);
                        runtime.SetPos(d.line, d.col, d.source);

                        if (scope.IsGlobal())
                        {
                            scope.Define(d.name, new RunTimeVariable(d.type, value, true, d.isArray, d.visibility, d.ownermodule));
                        }
                        else
                        {
                            scope.Define(d.name, new RunTimeVariable(d.type, value, true, d.isArray));
                        }
                    }
                }
                else if (s is PushStatement push)
                {
                    object target = expressions.Evaluate(push.target, scope);

                    if (target is not List<object> values)
                    {
                        throw runtime.Error("push target is not an array");
                    }

                    object toPush = expressions.Evaluate(push.value, scope);
                    VarType elementType = GetArrayElementType(push.target, scope);

                    TypeChecker.CheckType(runtime, toPush, elementType);

                    values.Add(toPush);
                }
                else if (s is PopStatement pop)
                {
                    object target = expressions.Evaluate(pop.target, scope);

                    if (target is not List<object> values)
                    {
                        throw runtime.Error("pop target is not an array");
                    }

                    if (values.Count == 0)
                    {
                        throw runtime.Error("Cannot pop from an empty array");
                    }

                    values.RemoveAt(values.Count - 1);
                }
                else if (s is TryParseStatement tp)
                {
                    object toParse = expressions.Evaluate(tp.value, scope);

                    RunTimeVariable target = scope.Get(tp.target);
                    RunTimeVariable success = scope.Get(tp.success);

                    runtime.SetPos(tp.line, tp.col, tp.source);

                    if (success.type != VarType.BOOL || success.isArray)
                    {
                        throw runtime.Error($"Type error: expected {VarType.BOOL} but got {success.type}");
                    }

                    bool parsed = false;
                    object? parsedValue = null;

                    if (tp.type == VarType.NUM)
                    {
                        if (Conversions.TryToNum(toParse, out int num))
                        {
                            parsed = true;
                            parsedValue = num;
                        }
                    }
                    else if (tp.type == VarType.DECIMAL)
                    {
                        if (Conversions.TryToDecimal(toParse, out decimal dec))
                        {
                            parsed = true;
                            parsedValue = dec;
                        }
                    }
                    else if (tp.type == VarType.CHAR)
                    {
                        if (Conversions.TryToChar(toParse, out char ch))
                        {
                            parsed = true;
                            parsedValue = ch;
                        }
                    }
                    else if (tp.type == VarType.BOOL)
                    {
                        if (Conversions.TryToBool(toParse, out bool boolean))
                        {
                            parsed = true;
                            parsedValue = boolean;
                        }
                    }

                    if (parsed)
                    {
                        TypeChecker.CheckType(runtime, parsedValue!, target.type, target.isArray);

                        target.value = parsedValue;
                        target.isInitialized = true;

                        success.value = true;
                        success.isInitialized = true;
                    }
                    else
                    {
                        success.value = false;
                        success.isInitialized = true;
                    }
                }
                else if (s is Show sh)
                {
                    if (sh.expression is null)
                    {
                        Console.Write("", CultureInfo.InvariantCulture);
                        continue;
                    }
                    object result = expressions.Evaluate(sh.expression, scope);
                    Console.Write(result);
                }
                else if (s is ShowLn shln)
                {
                    if (shln.expression is null)
                    {
                        Console.WriteLine("", CultureInfo.InvariantCulture);
                        continue;
                    }
                    object result = expressions.Evaluate(shln.expression, scope);
                    Console.WriteLine(result);
                }
                else if (s is DefFunction f)
                {
                    runtime.DefineFunction(f);
                }
                else if (s is CallStatement c)
                {
                    expressions.Evaluate(c.call, scope);
                }
                else if (s is IfStatement i)
                {
                    ExecResult? ifResult = ExecuteIfStatement(i, scope);

                    if (ifResult != null) return ifResult;
                }
                else if (s is ExpressionStatement exp)
                {
                    expressions.Evaluate(exp.expression, scope);
                }
                else if (s is WhileStatement w)
                {
                    ExecResult? whileResult = ExecuteWhileStatement(w, scope);

                    if (whileResult != null) return whileResult;
                }
                else if (s is ForStatement fo)
                {
                    ExecResult? forResult = ExecuteForStatement(fo, scope);

                    if (forResult != null) return forResult;
                }
                else if (s is ForEveryStatement fe)
                {
                    ExecResult? forEveryResult = ExecuteForEveryStatement(fe, scope);

                    if (forEveryResult != null) return forEveryResult;
                }
                else if (s is ReturnStatement ret)
                {
                    if (ret.expression is null)
                    {
                        return new ExecResult(ExecFlow.Return);
                    }

                    object result = expressions.Evaluate(ret.expression, scope);
                    runtime.SetPos(ret.line, ret.col, ret.source);

                    return new ExecResult(ExecFlow.Return, result);
                }
                else if (s is BreakStatement)
                {
                    return new ExecResult(ExecFlow.Break);
                }
                else if (s is ContinueStatement)
                {
                    return new ExecResult(ExecFlow.Continue);
                }
                else if (s is DefClass defC)
                {
                    runtime.DefineClass(defC);
                }
                else if (s is RedefineTarget redef)
                {
                    if (redef.value is NewExpression)
                    {
                        Uninitialize(redef.target, scope);
                        continue;
                    }

                    object value = expressions.Evaluate(redef.value, scope);

                    Assign(redef.target, value, scope);
                }
                else if (s is DefineObject defObj)
                {
                    object value = expressions.Evaluate(defObj.value, scope);

                    if (value is not ClassInstance instance)
                    {
                        throw runtime.Error($"Expected object of class {defObj.className}");
                    }

                    if (instance.definition.name != defObj.className)
                    {
                        throw runtime.Error($"Expected object of class {defObj.className} " + $"but received {instance.definition.name}");
                    }

                    string? ownerModule = null;

                    if (scope.IsGlobal())
                    {
                        ownerModule = runtime.moduleLoader.CurrentFile;
                    }

                    scope.DefineObject(defObj.name, instance, defObj.visibility, ownerModule);
                }
                else
                {
                    throw runtime.Error($"Invalid statement {s}");
                }
            }

            return null;
        }

        private void Uninitialize(Expression target, Scope scope)
        {
            if (target is VarExpression variable)
            {
                RunTimeVariable runtimeVariable = scope.Get(variable.name);
                runtimeVariable.value = null;
                runtimeVariable.isInitialized = false;

                return;
            }

            if (target is SelfField selfField)
            {
                ClassInstance? instance = scope.currentInstance;

                if (instance == null)
                {
                    throw runtime.Error("self can only be used inside a class");
                }

                RunTimeVariable field = instance.fields.GetLocal(selfField.fieldName);
                field.value = null;
                field.isInitialized = false;

                return;
            }

            if (target is FieldAccess fieldAccess)
            {
                object owner = expressions.Evaluate(fieldAccess.target, scope);

                if (owner is not ClassInstance instance)
                {
                    throw runtime.Error("Cannot uninitialize field on a non-object value");
                }

                RunTimeVariable field = instance.fields.GetLocal(fieldAccess.fieldName);
                field.value = null;
                field.isInitialized = false;

                return;
            }

            throw runtime.Error("This target cannot be set to new");
        }

        private VarType GetArrayElementType(Expression target, Scope scope)
        {
            if (target is VarExpression variable)
            {
                RunTimeVariable array = scope.Get(variable.name);

                if (!array.isArray)
                {
                    throw runtime.Error($"Variable {variable.name} is not an array");
                }

                return array.type;
            }

            if (target is SelfField selfField)
            {
                ClassInstance? instance = scope.currentInstance;

                if (instance == null)
                {
                    throw runtime.Error("self can only be used inside a class");
                }

                RunTimeVariable array = instance.fields.GetLocal(selfField.fieldName);

                if (!array.isArray)
                {
                    throw runtime.Error($"Field {selfField.fieldName} is not an array");
                }

                return array.type;
            }

            if (target is FieldAccess fieldAccess)
            {
                object owner =
                    expressions.Evaluate(fieldAccess.target, scope
);

                if (owner is not ClassInstance instance)
                {
                    throw runtime.Error("Array field belongs to a non-object value");
                }

                RunTimeVariable array = instance.fields.GetLocal(fieldAccess.fieldName);

                if (!array.isArray)
                {
                    throw runtime.Error($"Field {fieldAccess.fieldName} is not an array");
                }

                return array.type;
            }

            throw runtime.Error("Unable to determine array element type");
        }

        private void AssignArrayElement(ArrayAccessExpression access, object value, Scope scope)
        {
            object target = expressions.Evaluate(access.target, scope);

            if (target is not List<object> values)
            {
                throw runtime.Error("Target is not an array");
            }

            object indexValue = expressions.Evaluate(access.index, scope);

            if (indexValue is not int index)
            {
                throw runtime.Error("Array index must be NUM");
            }

            if (index < 0 || index >= values.Count)
            {
                throw runtime.Error($"Array index {index} is out of bounds");
            }

            VarType elementType = GetArrayElementType(access.target, scope);

            TypeChecker.CheckType(runtime, value, elementType);

            values[index] = value;
        }

        private void Assign(Expression target, object value, Scope scope)
        {
            if (target is VarExpression variable)
            {
                RunTimeVariable runtimeVariable = scope.Get(variable.name);

                TypeChecker.CheckType(runtime, value, runtimeVariable.type, runtimeVariable.isArray);

                runtimeVariable.value = value;
                runtimeVariable.isInitialized = true;

                return;
            }

            if (target is SelfField selfField)
            {
                ClassInstance? instance = scope.currentInstance;

                if (instance == null)
                {
                    throw runtime.Error("self can only be used inside a class");
                }

                DefineVar? primitiveField = instance.definition.fields.FirstOrDefault(f => f.name == selfField.fieldName);

                DefineObject? objectField = instance.definition.objectFields.FirstOrDefault(f => f.name == selfField.fieldName);

                if (primitiveField == null && objectField == null)
                {
                    throw runtime.Error($"Field {selfField.fieldName} can't be found");
                }

                if (primitiveField != null)
                {
                    TypeChecker.CheckType(runtime, value, primitiveField.type, primitiveField.isArray);
                    RunTimeVariable selfVariable = instance.fields.GetLocal(selfField.fieldName);

                    selfVariable.value = value;
                    selfVariable.isInitialized = true;

                    return;
                }

                if (objectField != null)
                {
                    if (value is not ClassInstance objectValue)
                    {
                        throw runtime.Error($"Field {selfField.fieldName} expected object " + $"of class {objectField.className}");
                    }

                    if (objectValue.definition.name != objectField.className)
                    {
                        throw runtime.Error($"Field {selfField.fieldName} expected " + $"{objectField.className} but received " + $"{objectValue.definition.name}");
                    }

                    instance.fields.SetObjectLocal(selfField.fieldName, objectValue);

                    return;
                }
            }

            if (target is FieldAccess fieldAccess)
            {
                object owner = expressions.Evaluate(fieldAccess.target, scope);

                if (owner is not ClassInstance instance)
                {
                    throw runtime.Error($"Cannot redefine field {fieldAccess.fieldName} on a non-object value");
                }

                DefineVar? primitiveDefinition = instance.definition.fields.FirstOrDefault(f => f.name == fieldAccess.fieldName);
                DefineObject? objectDefinition = instance.definition.objectFields.FirstOrDefault(f => f.name == fieldAccess.fieldName);

                if (primitiveDefinition == null && objectDefinition == null)
                {
                    throw runtime.Error($"Class {instance.definition.name} has no field {fieldAccess.fieldName}");
                }

                Visibility? visibility = primitiveDefinition?.visibility ?? objectDefinition?.visibility;

                if (visibility == Visibility.PRIVATE)
                {
                    ClassInstance? caller = scope.currentInstance;

                    if (caller == null || caller.definition != instance.definition)
                    {
                        throw runtime.Error($"Field {fieldAccess.fieldName} is private");
                    }
                }

                if (primitiveDefinition != null)
                {
                    RunTimeVariable field = instance.fields.GetLocal(fieldAccess.fieldName);

                    TypeChecker.CheckType(runtime, value, field.type, field.isArray);

                    field.value = value;
                    field.isInitialized = true;
                    return;
                }

                if (objectDefinition != null)
                {
                    if (value is not ClassInstance objectValue)
                    {
                        throw runtime.Error($"Expected object of class {objectDefinition.className}");
                    }

                    if (objectValue.definition.name != objectDefinition.className)
                    {
                        throw runtime.Error($"Expected object of class {objectDefinition.className} " + $"but received {objectValue.definition.name}");
                    }

                    instance.fields.SetObjectLocal(fieldAccess.fieldName, objectValue);

                    return;
                }
            }

            if (target is ArrayAccessExpression arrayAccess)
            {
                AssignArrayElement(arrayAccess, value, scope);
                return;
            }

            throw runtime.Error("Invalid redefine target");
        }

        private ExecResult? ExecuteIfStatement(IfStatement ifStatement, Scope currentScope)
        {
            object conditionValue = expressions.Evaluate(ifStatement.condition, currentScope);

            TypeChecker.CheckType(runtime, conditionValue, VarType.BOOL);

            bool condition = Convert.ToBoolean(conditionValue);

            if (condition)
            {
                return Execute(ifStatement.body, currentScope.CreateChild());
            }

            if (ifStatement.elseIfStatements != null)
            {
                foreach (ElseIfStatement eis in ifStatement.elseIfStatements)
                {
                    object elseIfConditionValue = expressions.Evaluate(eis.condition, currentScope);
                    TypeChecker.CheckType(runtime, elseIfConditionValue, VarType.BOOL);
                    bool elseIfCondition = Convert.ToBoolean(elseIfConditionValue);

                    if (elseIfCondition)
                    {
                        return Execute(eis.body, currentScope.CreateChild());
                    }
                }
            }

            if (ifStatement.elseBody != null)
            {
                return Execute(ifStatement.elseBody, currentScope.CreateChild());
            }

            return null;
        }

        private ExecResult? ExecuteWhileStatement(WhileStatement whileStatement, Scope currentScope)
        {
            object conditionValue = expressions.Evaluate(whileStatement.expression, currentScope);
            TypeChecker.CheckType(runtime, conditionValue, VarType.BOOL);

            bool condition = Convert.ToBoolean(conditionValue);

            while (condition)
            {
                ExecResult? result = Execute(whileStatement.body, currentScope.CreateChild());

                if (result != null)
                {
                    if (result.Flow == ExecFlow.Return) return result;

                    if (result.Flow == ExecFlow.Break) break;
                }

                conditionValue = expressions.Evaluate(whileStatement.expression, currentScope);
                TypeChecker.CheckType(runtime, conditionValue, VarType.BOOL);
                condition = Convert.ToBoolean(conditionValue);
            }

            return null;
        }

        private ExecResult? ExecuteForStatement(ForStatement forStatement, Scope currentScope)
        {
            Scope loopScope = currentScope.CreateChild();

            List<Statement> initialization = new() { forStatement.initialization };
            List<Statement> step = new() { forStatement.step };

            Execute(initialization, loopScope);

            object conditionValue = expressions.Evaluate(forStatement.condition, loopScope);
            TypeChecker.CheckType(runtime, conditionValue, VarType.BOOL);

            bool condition = Convert.ToBoolean(conditionValue);

            while (condition)
            {
                ExecResult? result = Execute(forStatement.body, loopScope.CreateChild());

                if (result != null)
                {
                    if (result.Flow == ExecFlow.Return) return result;

                    if (result.Flow == ExecFlow.Break) break;
                }

                Execute(step, loopScope);

                conditionValue = expressions.Evaluate(forStatement.condition, loopScope);
                TypeChecker.CheckType(runtime, conditionValue, VarType.BOOL);
                condition = Convert.ToBoolean(conditionValue);
            }

            return null;
        }

        private ExecResult? ExecuteForEveryStatement(ForEveryStatement forEveryStatement, Scope currentScope)
        {
            object target = expressions.Evaluate(forEveryStatement.target, currentScope);

            if (target is not List<object> values)
            {
                throw runtime.Error("forevery target is not an array");
            }

            for (int i = 0; i < values.Count; i++)
            {
                object element = values[i];

                TypeChecker.CheckType(runtime, element, forEveryStatement.type, forEveryStatement.isArray);

                Scope loopScope = currentScope.CreateChild();
                loopScope.Define(forEveryStatement.name, new RunTimeVariable(forEveryStatement.type, element, true, forEveryStatement.isArray));
                ExecResult? result = Execute(forEveryStatement.body, loopScope);

                if (result != null)
                {
                    if (result.Flow == ExecFlow.Return)
                    {
                        return result;
                    }

                    if (result.Flow == ExecFlow.Break)
                    {
                        break;
                    }

                    if (result.Flow == ExecFlow.Continue)
                    {
                        continue;
                    }
                }
            }

            return null;
        }
    }
}
