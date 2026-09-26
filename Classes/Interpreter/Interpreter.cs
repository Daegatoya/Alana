using Classes.AST;
using Classes.AST.Expressions;
using Classes.AST.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using static System.Formats.Asn1.AsnWriter;

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
                else if (s is IfStatement i)
                {
                    Dictionary<string, object> local_variables = new();
                    ExecuteIfStatement(i, local_variables);
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
                else if (s is IfStatement i)
                {
                    object? ifResult = ExecuteIfStatement(i, local_variables);
                    if (ifResult != null) return ifResult;
                }
                else
                {
                    throw new Exception($"Invalid statement {s}");
                }
            }

            return null;
        }

        private object? ExecuteIfStatement(IfStatement ifStatement, Dictionary<string, object> currentScope)
        {
            object conditionValue = Evaluate(ifStatement.condition, currentScope);

            CheckType(conditionValue, VarType.BOOL);

            bool condition = Convert.ToBoolean(conditionValue);

            if (condition)
            {
                return ExecuteIf(ifStatement.body, currentScope);
            }

            if (ifStatement.elseIfStatements != null)
            {
                foreach (ElseIfStatement eis in ifStatement.elseIfStatements)
                {
                    object elseIfConditionValue = Evaluate(eis.condition, currentScope);
                    CheckType(elseIfConditionValue, VarType.BOOL);
                    bool elseIfCondition = Convert.ToBoolean(elseIfConditionValue);

                    if (elseIfCondition)
                    {
                        return ExecuteIf(eis.body, currentScope);
                    }
                }
            }

            if(ifStatement.elseBody != null)
            {
                return ExecuteIf(ifStatement.elseBody, currentScope);
            }

            return null;
        }

        private object ExecuteIf(List<Statement> statements, Dictionary<string, object> local_variables)
        {
            Dictionary<string, object> scope = new();
            foreach (KeyValuePair<string, object> kvp in local_variables)
            {
                scope.Add(kvp.Key, kvp.Value);
            }
            if (statements is null) return null;
            foreach (Statement s in statements)
            {
                if (s is DefineVar d)
                {
                    object value = Evaluate(d.value, scope);
                    scope[d.name] = value;
                }
                else if (s is Show sh)
                {
                    object result = Evaluate(sh.expression, scope);
                    Console.WriteLine(result);
                }
                else if (s is ReturnStatement r)
                {
                    if (r.expression is null)
                    {
                        return null;
                    }
                    object result = Evaluate(r.expression, scope);

                    return result;
                }
                else if (s is IfStatement i)
                {
                    object? ifResult = ExecuteIfStatement(i, scope);
                    if (ifResult != null) return ifResult;
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
                        Expression arg = c.arguments[i];
                        object a_value = Evaluate(arg, local_variables);

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

            if(expression is AndExpression a)
            {
                object left = Evaluate(a.left, local_variables);
                object right = Evaluate(a.right, local_variables);

                if (left is bool leftBool && right is bool rightBool) return (leftBool && rightBool);

                throw new Exception($"Comparison error: cannot compare {left.GetType().Name} with {right.GetType().Name} using And");
            }

            if (expression is OrExpression o)
            {
                object left = Evaluate(o.left, local_variables);
                object right = Evaluate(o.right, local_variables);

                if (left is bool leftBool && right is bool rightBool) return (leftBool || rightBool);

                throw new Exception($"Comparison error: cannot compare {left.GetType().Name} with {right.GetType().Name} using Or");
            }

            if (expression is NotExpression not)
            {
                object intern = Evaluate(not.expression, local_variables);

                if (intern is bool internBool) return !internBool;

                throw new Exception($"Type error: NOT expected BOOL but got {intern.GetType().Name}");
            }

            if (expression is SameAs s)
            {
                object left = Evaluate(s.left, local_variables);
                object right = Evaluate(s.right, local_variables);

                if (left is int leftInt && right is int rightInt) return leftInt == rightInt;
                if (left is decimal leftDecimal && right is decimal rightDecimal) return leftDecimal == rightDecimal;
                if (left is int leftIntDecimal && right is decimal rightDecimalInt) return leftIntDecimal == rightDecimalInt;
                if (left is decimal leftDecimalInt && right is int rightIntDecimal) return leftDecimalInt == rightIntDecimal;
                if (left is string leftString && right is string rightString) return leftString == rightString;
                if (left is char leftChar && right is char rightChar) return leftChar == rightChar;
                if (left is bool leftBool && right is bool rightBool) return leftBool == rightBool;

                throw new Exception($"Comparison error: cannot compare {left.GetType().Name} with {right.GetType().Name} using SameAs");
            }

            if (expression is GreaterThan g)
            {
                object left = Evaluate(g.left, local_variables);
                object right = Evaluate(g.right, local_variables);

                if (left is int leftInt && right is int rightInt) return leftInt > rightInt;
                if (left is decimal leftDecimal && right is decimal rightDecimal) return leftDecimal > rightDecimal;
                if (left is int leftIntDecimal && right is decimal rightDecimalInt) return leftIntDecimal > rightDecimalInt;
                if (left is decimal leftDecimalInt && right is int rightIntDecimal) return leftDecimalInt > rightIntDecimal;

                throw new Exception($"Comparison error: cannot compare {left.GetType().Name} with {right.GetType().Name} using GreaterThan");
            }

            if (expression is GreaterOrEqual goe)
            {
                object left = Evaluate(goe.left, local_variables);
                object right = Evaluate(goe.right, local_variables);

                if (left is int leftInt && right is int rightInt) return leftInt >= rightInt;
                if (left is decimal leftDecimal && right is decimal rightDecimal) return leftDecimal >= rightDecimal;
                if (left is int leftIntDecimal && right is decimal rightDecimalInt) return leftIntDecimal >= rightDecimalInt;
                if (left is decimal leftDecimalInt && right is int rightIntDecimal) return leftDecimalInt >= rightIntDecimal;

                throw new Exception($"Comparison error: cannot compare {left.GetType().Name} with {right.GetType().Name} using GreaterOrEqual");
            }

            if (expression is LessThan l)
            {
                object left = Evaluate(l.left, local_variables);
                object right = Evaluate(l.right, local_variables);

                if (left is int leftInt && right is int rightInt) return leftInt < rightInt;
                if (left is decimal leftDecimal && right is decimal rightDecimal) return leftDecimal < rightDecimal;
                if (left is int leftIntDecimal && right is decimal rightDecimalInt) return leftIntDecimal < rightDecimalInt;
                if (left is decimal leftDecimalInt && right is int rightIntDecimal) return leftDecimalInt < rightIntDecimal;

                throw new Exception($"Comparison error: cannot compare {left.GetType().Name} with {right.GetType().Name} using LessThan");
            }

            if (expression is LessOrEqual loe)
            {
                object left = Evaluate(loe.left, local_variables);
                object right = Evaluate(loe.right, local_variables);

                if (left is int leftInt && right is int rightInt) return leftInt <= rightInt;
                if (left is decimal leftDecimal && right is decimal rightDecimal) return leftDecimal <= rightDecimal;
                if (left is int leftIntDecimal && right is decimal rightDecimalInt) return leftIntDecimal <= rightDecimalInt;
                if (left is decimal leftDecimalInt && right is int rightIntDecimal) return leftDecimalInt <= rightIntDecimal;

                throw new Exception($"Comparison error: cannot compare {left.GetType().Name} with {right.GetType().Name} using LessOrEqual");
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
