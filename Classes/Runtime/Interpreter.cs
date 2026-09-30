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
        private Scope globals;
        private ExpressionEvaluator evaluator;
        private StatementExecutor executor;

        internal int line, col;
        internal string source = string.Empty;

        public Interpreter()
        {
            evaluator = new ExpressionEvaluator(this);
            executor = new StatementExecutor(this, evaluator);
            globals = new Scope(this);
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

        internal void DefineFunction(DefFunction function)
        {
            if (functions.ContainsKey(function.name))
            {
                throw Error($"Function {function.name} is already defined");
            }

            functions.Add(function.name, function);
        }

        internal object? Invoke(FuncCall call, Scope callerScope)
        {
            if (functions.TryGetValue(call.name, out DefFunction? function))
            {
                Scope scope = globals.CreateChild();

                for (int i = 0; i < function.parameters!.Count; i++)
                {
                    Param p = function.parameters[i];
                    Expression arg = call.arguments[i];
                    object a_value = evaluator.Evaluate(arg, callerScope);

                    if (!TypeChecker.CheckType(this, a_value, p.type, p.isArray))
                    {
                        throw Error($"Parsed argument {a_value} has the wrong type for parameter {p.name} of type {p.type}");
                    }
                    scope.Define(p.name, new RunTimeVariable(p.type, a_value, true, p.isArray));
                }

                ExecResult? result = executor.Execute(function.body, scope);

                if (result == null)
                {
                    return null;
                }

                if (result.Flow == ExecFlow.Return)
                {
                    return result.Value;
                }

                throw FlowError(result.Flow);
            }
            else
            {
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
