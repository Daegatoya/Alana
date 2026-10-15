using Classes.AST;
using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Runtime
{
    internal class ExpressionEvaluator
    {
        private Interpreter runtime;

        public ExpressionEvaluator(Interpreter runtime)
        {
            this.runtime = runtime;
        }

        public object Evaluate(Expression expression, Scope scope)
        {
            runtime.SetPos(expression.line, expression.col, expression.source);

            if (expression is ArrayAccessExpression arrayAccess)
            {
                object target = Evaluate(arrayAccess.target, scope);

                object indexValue = Evaluate(arrayAccess.index, scope);

                if (indexValue is not int index)
                {
                    throw runtime.Error("Array index must be NUM");
                }

                if (target is not List<object> values)
                {
                    throw runtime.Error("Value is not an array");
                }

                if (index < 0 || index >= values.Count)
                {
                    throw runtime.Error($"Array index {index} is out of bounds");
                }

                return values[index];
            }

            if (expression is FieldAccess fieldAccess)
            {
                object target = Evaluate(fieldAccess.target, scope);

                if (target is not ClassInstance instance)
                {
                    throw runtime.Error($"Cannot access field {fieldAccess.fieldName} on a non-object value");
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
                    ClassInstance? callerInstance = scope.currentInstance;

                    if (callerInstance == null || callerInstance.definition != instance.definition)
                    {
                        throw runtime.Error($"Field {fieldAccess.fieldName} is private");
                    }
                }

                if (primitiveDefinition != null)
                {
                    RunTimeVariable field = instance.fields.GetLocal(fieldAccess.fieldName);

                    if (!field.isInitialized)
                    {
                        throw runtime.Error($"Field {fieldAccess.fieldName} is not initialized");
                    }

                    return field.value!;
                }

                ClassInstance objectField = instance.fields.GetObjectLocal(fieldAccess.fieldName);
                return objectField;
            }

            if (expression is ArrayExpression array)
            {
                List<object> values = new();

                foreach (Expression element in array.values)
                {
                    values.Add(Evaluate(element, scope));
                }

                return values;
            }

            if (expression is FuncCall c)
            {
                return runtime.Invoke(c, scope)!;
            }

            if (expression is MethodCall methodCall)
            {
                return runtime.InvokeMethod(methodCall, scope);
            }

            if (expression is StringExpression str)
            {
                return str.value;
            }

            if (expression is ReadLine)
            {
                return Console.ReadLine()!;
            }

            if (expression is ReadKey)
            {
                return Console.ReadKey(intercept: true).KeyChar;
            }

            if (expression is BoolExpression b)
            {
                return b.value;
            }

            if (expression is CharExpression ch)
            {
                return ch.value;
            }

            if (expression is DecimalExpression de)
            {
                return de.value;
            }

            if (expression is NumberExpression number)
            {
                return number.value;
            }

            if (expression is SelfField selfField)
            {
                ClassInstance? instance = scope.currentInstance;

                if (instance == null)
                {
                    throw runtime.Error("self can only be used inside a class");
                }

                DefineVar? primitiveDefinition = instance.definition.fields.FirstOrDefault(f => f.name == selfField.fieldName);

                if (primitiveDefinition != null)
                {
                    RunTimeVariable field = instance.fields.GetLocal(selfField.fieldName);

                    if (!field.isInitialized)
                    {
                        throw runtime.Error($"Field {selfField.fieldName} is not initialized");
                    }

                    return field.value!;
                }

                DefineObject? objectDefinition = instance.definition.objectFields.FirstOrDefault(f => f.name == selfField.fieldName);

                if (objectDefinition != null)
                {
                    return instance.fields.GetObjectLocal(selfField.fieldName);
                }

                throw runtime.Error($"Class {instance.definition.name} has no field {selfField.fieldName}");
            }

            if (expression is VarExpression variable)
            {
                if (variable.name == "self")
                {
                    return scope.currentInstance ?? throw runtime.Error("self can only be used inside a class");
                }

                RunTimeVariable? primitive = scope.Find(variable.name);

                if (primitive != null)
                {
                    if (!primitive.isInitialized)
                    {
                        throw runtime.Error($"Variable {variable.name} is not initialized");
                    }

                    return primitive.value!;
                }

                ClassInstance? instance = scope.FindObject(variable.name);

                if (instance != null)
                {
                    return instance;
                }

                throw runtime.Error($"Variable or object {variable.name} can't be found");
            }

            if (expression is ParseNum pnum)
            {
                object value = Evaluate(pnum.expression, scope);
                runtime.SetPos(pnum.line, pnum.col, pnum.source);
                return Conversions.ToNum(runtime, value);
            }

            if (expression is ParseDecimal pdec)
            {
                object value = Evaluate(pdec.expression, scope);
                runtime.SetPos(pdec.line, pdec.col, pdec.source);
                return Conversions.ToDecimal(runtime, value);
            }

            if (expression is ParseBool pbool)
            {
                object value = Evaluate(pbool.expression, scope);
                runtime.SetPos(pbool.line, pbool.col, pbool.source);
                return Conversions.ToBool(runtime, value);
            }

            if (expression is ParseChar pchar)
            {
                object value = Evaluate(pchar.expression, scope);
                runtime.SetPos(pchar.line, pchar.col, pchar.source);
                return Conversions.ToChar(runtime, value);
            }

            if (expression is ParseString pstr)
            {
                object value = Evaluate(pstr.expression, scope);
                runtime.SetPos(pstr.line, pstr.col, pstr.source);
                return Conversions.ToStr(value);
            }

            if (expression is Length length)
            {
                object target = Evaluate(length.target, scope);

                if (target is not List<object> values)
                {
                    throw runtime.Error("length can only be used on an array");
                }

                return values.Count;
            }

            if (expression is NewObject newO)
            {
                return runtime.InstantiateClass(newO.className, newO.arguments, scope);
            }

            if (expression is AndExpression a)
            {
                object left = Evaluate(a.left, scope);
                object right = Evaluate(a.right, scope);

                runtime.SetPos(a.line, a.col, a.source);
                return Operators.And(runtime, left, right);
            }

            if (expression is OrExpression o)
            {
                object left = Evaluate(o.left, scope);
                object right = Evaluate(o.right, scope);

                runtime.SetPos(o.line, o.col, o.source);
                return Operators.Or(runtime, left, right);
            }

            if (expression is NotExpression not)
            {
                object intern = Evaluate(not.expression, scope);

                runtime.SetPos(not.line, not.col, not.source);
                return Operators.Not(runtime, intern);
            }

            if (expression is SameAs s)
            {
                object left = Evaluate(s.left, scope);
                object right = Evaluate(s.right, scope);

                runtime.SetPos(s.line, s.col, s.source);
                return Operators.SameAs(runtime, left, right);
            }

            if (expression is GreaterThan g)
            {
                object left = Evaluate(g.left, scope);
                object right = Evaluate(g.right, scope);

                runtime.SetPos(g.line, g.col, g.source);
                return Operators.GreaterThan(runtime, left, right);
            }

            if (expression is GreaterOrEqual goe)
            {
                object left = Evaluate(goe.left, scope);
                object right = Evaluate(goe.right, scope);

                runtime.SetPos(goe.line, goe.col, goe.source);
                return Operators.GreaterOrEqual(runtime, left, right);
            }

            if (expression is LessThan l)
            {
                object left = Evaluate(l.left, scope);
                object right = Evaluate(l.right, scope);

                runtime.SetPos(l.line, l.col, l.source);
                return Operators.LessThan(runtime, left, right);
            }

            if (expression is NegateExpression neg)
            {
                object value = Evaluate(neg.expression, scope);

                if (value is int num)
                {
                    return -num;
                }

                if (value is decimal decimalNumber)
                {
                    return -decimalNumber;
                }

                throw runtime.Error("Unary minus can only be used with NUM or DECIMAL");
            }

            if (expression is LessOrEqual loe)
            {
                object left = Evaluate(loe.left, scope);
                object right = Evaluate(loe.right, scope);

                runtime.SetPos(loe.line, loe.col, loe.source);
                return Operators.LessOrEqual(runtime, left, right);
            }

            if (expression is Addition addition)
            {
                object left = Evaluate(addition.left, scope);
                object right = Evaluate(addition.right, scope);

                object? result = Operators.Add(left, right);

                if (result != null) return result;
            }
            else if (expression is Substraction substraction)
            {
                object left = Evaluate(substraction.left, scope);
                object right = Evaluate(substraction.right, scope);

                object? result = Operators.Substract(left, right);

                if (result != null) return result;
            }
            else if (expression is Multiplication multiplication)
            {
                object left = Evaluate(multiplication.left, scope);
                object right = Evaluate(multiplication.right, scope);

                object? result = Operators.Multiply(left, right);

                if (result != null) return result;
            }
            else if (expression is Division division)
            {
                object left = Evaluate(division.left, scope);
                object right = Evaluate(division.right, scope);

                object? result = Operators.Divide(left, right);

                if (result != null) return result;
            }
            else if (expression is Modulo modulo)
            {
                object left = Evaluate(modulo.left, scope);
                object right = Evaluate(modulo.right, scope);

                object? result = Operators.Modulo(left, right);

                if (result != null) return result;
            }

            runtime.SetPos(expression.line, expression.col, expression.source);
            throw runtime.Error("Unknown expression");
        }
    }
}
