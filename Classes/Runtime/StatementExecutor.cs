using Classes.AST;
using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
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

                if (s is DefineVar d)
                {
                    if (d.value is NewExpression)
                    {
                        scope.Define(d.name, new RunTimeVariable(d.type, null, false, d.isArray));
                    }
                    else
                    {
                        object value = expressions.Evaluate(d.value!, scope);
                        TypeChecker.CheckType(runtime, value, d.type, d.isArray);
                        runtime.SetPos(d.line, d.col, d.source);
                        scope.Define(d.name, new RunTimeVariable(d.type, value, true, d.isArray));
                    }
                }
                else if (s is RedefineVar r)
                {
                    RunTimeVariable variable = scope.Get(r.name);

                    if (r.value is NewExpression)
                    {
                        variable.value = null;
                        variable.isInitialized = false;
                        continue;
                    }

                    object value = expressions.Evaluate(r.value!, scope);
                    TypeChecker.CheckType(runtime, value, variable.type, variable.isArray);

                    variable.value = value;
                    variable.isInitialized = true;
                }
                else if (s is PushStatement push)
                {
                    RunTimeVariable variable = scope.Get(push.name);

                    if (!variable.isArray)
                    {
                        throw runtime.Error($"Variable {push.name} is not an array");
                    }
                    if (!variable.isInitialized)
                    {
                        throw runtime.Error($"Variable {push.name} is not initialized");
                    }
                    if (variable.value is not List<object> values)
                    {
                        throw runtime.Error($"Variable {push.name} does not contain a valid array");
                    }

                    object toPush = expressions.Evaluate(push.value, scope);
                    TypeChecker.CheckType(runtime, toPush, variable.type);
                    values.Add(toPush);
                }
                else if (s is PopStatement pop)
                {
                    RunTimeVariable variable = scope.Get(pop.name);

                    if (!variable.isArray)
                    {
                        throw runtime.Error($"Variable {pop.name} is not an array");
                    }
                    if (!variable.isInitialized)
                    {
                        throw runtime.Error($"Variable {pop.name} is not initialized");
                    }
                    if (variable.value is not List<object> values)
                    {
                        throw runtime.Error($"Variable {pop.name} does not contain a valid array");
                    }
                    if (values.Count == 0)
                    {
                        throw runtime.Error($"Cannot pop from empty array {pop.name}");
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
                else if (s is RedefineArrElement arr)
                {
                    RunTimeVariable variable = scope.Get(arr.name);

                    if (!variable.isArray)
                    {
                        throw runtime.Error($"Variable {arr.name} is not an array");
                    }
                    if (!variable.isInitialized)
                    {
                        throw runtime.Error($"Variable {arr.name} is not initialized");
                    }
                    object indexValue = expressions.Evaluate(arr.index, scope);

                    if (indexValue is not int index)
                    {
                        throw runtime.Error($"Array index must be NUM");
                    }
                    if (variable.value is not List<object> values)
                    {
                        throw runtime.Error($"Variable {arr.name} does not contain a valid array");
                    }
                    if (index < 0 || index >= values.Count)
                    {
                        throw runtime.Error($"Array index {index} is out of bounds");
                    }

                    object value = expressions.Evaluate(arr.value, scope);
                    TypeChecker.CheckType(runtime, value, variable.type);
                    values[index] = value;
                }
                else if (s is Show sh)
                {
                    if (sh.expression is null)
                    {
                        Console.Write("");
                        continue;
                    }
                    object result = expressions.Evaluate(sh.expression, scope);
                    Console.Write(result);
                }
                else if (s is ShowLn shln)
                {
                    if (shln.expression is null)
                    {
                        Console.WriteLine("");
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
                else
                {
                    throw runtime.Error($"Invalid statement {s}");
                }
            }

            return null;
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
            RunTimeVariable variable = currentScope.Get(forEveryStatement.array);

            if (!variable.isArray)
            {
                throw runtime.Error($"Variable {forEveryStatement.array} is not an array");
            }
            if (!variable.isInitialized)
            {
                throw runtime.Error($"Variable {forEveryStatement.array} is not initialized");
            }
            if (variable.value is not List<object> values)
            {
                throw runtime.Error($"Variable {forEveryStatement.array} does not contain a valid array");
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
                    if (result.Flow == ExecFlow.Return) return result;

                    if (result.Flow == ExecFlow.Break) break;
                }
            }

            return null;
        }
    }
}
