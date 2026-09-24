using Classes.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classes.AST.Expressions;
using Classes.AST.Models;
using System.Runtime.InteropServices.ObjectiveC;
using System.Windows.Markup;

namespace Classes.Interpreter
{
    public class Interpreter
    {
        private Dictionary<string, object> variables = new();
        private Dictionary<string, DefFunction> functions = new();

        public Interpreter() { }

        public void Execute(List<Statement> statements)
        {
            foreach (Statement s in statements)
            {
                if (s is DefineVar d)
                {
                    object value = Evaluate(d.value);
                    CheckType(value, d.type);
                    if (variables.ContainsKey(d.name))
                    {
                        variables[d.name] = value;
                    }
                    else
                    {
                        variables.Add(d.name, value);
                    }
                }
                else if (s is Show sh)
                {
                    object result = Evaluate(sh.expression);
                    Console.WriteLine(result);
                }
                else if (s is DefFunction f)
                {
                    functions.Add(f.name, f);
                }
                else if (s is CallStatement c)
                {
                    Evaluate(c.call);
                }
                else
                {
                    throw new Exception($"Invalid statement {s}");
                }
            }
        }

        private bool CheckType(object value, VarType? type)
        {
            bool isRight = type switch
            {
                VarType.NUM => value is int,
                VarType.STR => value is string,
                VarType.CHAR => value is char,
                VarType.DECIMAL => value is decimal,
                VarType.BOOL => value is bool,
                _ => false
            };

            if (isRight) return true;
            else throw new Exception($"Type error: expected {type} but got {value.GetType()}");
        }

        private object ExecuteFunc(List<Statement> statements, Dictionary<string, object> local_variables)
        {
            if (statements is null) return null;
            foreach (Statement s in statements)
            {
                if (s is DefineVar d)
                {
                    object value = Evaluate(d.value, local_variables);
                    local_variables[d.name] = value;
                }
                else if (s is Show sh)
                {
                    object result = Evaluate(sh.expression, local_variables);
                    Console.WriteLine(result);
                }
                else if (s is ReturnStatement r)
                {
                    if(r.expression is null)
                    {
                        return null;
                    }
                    object result = Evaluate(r.expression, local_variables);
                    return result;
                }
                else
                {
                    throw new Exception($"Invalid statement {s}");
                }
            }

            return null;
        }

        private object Evaluate(Expression expression, Dictionary<string, object>? local_variables = null)
        {
            if (expression is FuncCall c)
            {
                if (functions.TryGetValue(c.name, out DefFunction? function))
                {
                    Dictionary<string, object> scope = new();
                    for(int i = 0; i < function.parameters!.Count; i++)
                    {
                        Param p = function.parameters[i];
                        Expression a = c.arguments[i];
                        object a_value = Evaluate(a, local_variables);

                        if(!CheckType(a_value, p.type))
                        {
                            throw new Exception($"Parsed argument {a_value} has the wrong type for parameter {p.name} of type {p.type}");
                        }
                        scope.Add(p.name, a_value);
                    }

                    return ExecuteFunc(function.body!, scope);
                }
                else
                {
                    throw new Exception($"Unknown function {c.name}");
                }
            }
            if(expression is StringExpression str)
            {
                return str.value;
            }

            if(expression is BoolExpression b)
            {
                return b.value;
            }

            if(expression is CharExpression ch)
            {
                return ch.value;
            }

            if(expression is DecimalExpression de)
            {
                return de.value;
            }

            if (expression is NumberExpression number)
            {
                return number.value;
            }

            if (expression is VarExpression variable)
            {
                if(local_variables != null && local_variables.ContainsKey(variable.name))
                {
                    return local_variables[variable.name];
                }

                if (variables.ContainsKey(variable.name))
                {
                    return variables[variable.name];
                }

                throw new Exception($"Unknown variable {variable.name}");
            }

            if (expression is Addition addition)
            {
                object left = Evaluate(addition.left, local_variables);
                object right = Evaluate(addition.right, local_variables);

                if(left is int && right is int)
                {
                    int left_final = (int)left;
                    int right_final = (int)right;
                    return left_final + right_final;
                }

                else if(left is decimal && right is int)
                {
                    decimal left_final = (decimal)left;
                    int right_final = (int)right;
                    return left_final + right_final;
                }

                else if (left is int && right is decimal)
                {
                    int left_final = (int)left;
                    decimal right_final = (decimal)right;
                    return left_final + right_final;
                }
            }
            else if (expression is Substraction substraction)
            {
                object left = Evaluate(substraction.left, local_variables);
                object right = Evaluate(substraction.right, local_variables);

                if (left is int && right is int)
                {
                    int left_final = (int)left;
                    int right_final = (int)right;
                    return left_final - right_final;
                }

                else if (left is decimal && right is int)
                {
                    decimal left_final = (decimal)left;
                    int right_final = (int)right;
                    return left_final - right_final;
                }

                else if (left is int && right is decimal)
                {
                    int left_final = (int)left;
                    decimal right_final = (decimal)right;
                    return left_final - right_final;
                }
            }
            else if (expression is Multiplication multiplication)
            {
                object left = Evaluate(multiplication.left, local_variables);
                object right = Evaluate(multiplication.right, local_variables);

                if (left is int && right is int)
                {
                    int left_final = (int)left;
                    int right_final = (int)right;
                    return left_final * right_final;
                }

                else if (left is decimal && right is int)
                {
                    decimal left_final = (decimal)left;
                    int right_final = (int)right;
                    return left_final * right_final;
                }

                else if (left is int && right is decimal)
                {
                    int left_final = (int)left;
                    decimal right_final = (decimal)right;
                    return left_final * right_final;
                }
            }
            else if (expression is Division division)
            {
                object left = Evaluate(division.left, local_variables);
                object right = Evaluate(division.right, local_variables);

                if (left is int && right is int)
                {
                    int left_final = (int)left;
                    int right_final = (int)right;
                    return (decimal)left_final / right_final;
                }

                else if (left is decimal && right is int)
                {
                    decimal left_final = (decimal)left;
                    int right_final = (int)right;
                    return left_final / right_final;
                }

                else if (left is int && right is decimal)
                {
                    int left_final = (int)left;
                    decimal right_final = (decimal)right;
                    return left_final / right_final;
                }
            }

            throw new Exception("Unknown expression");
        }
    }
}
