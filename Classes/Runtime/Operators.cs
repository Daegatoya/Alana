using Classes.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Runtime
{
    internal static class Operators
    {
        public static object? Add(object left, object right)
        {
            if (left is int && right is int)
            {
                int left_final = (int)left;
                int right_final = (int)right;
                return left_final + right_final;
            }

            else if (left is decimal && right is int)
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

            else if (left is string && right is string)
            {
                string left_final = (string)left;
                string right_final = (string)right;
                return left_final + right_final;
            }

            else if (left is string && right is char)
            {
                string left_final = (string)left;
                char right_final = (char)right;
                return left_final + right_final;
            }

            else if (left is char && right is string)
            {
                char left_final = (char)left;
                string right_final = (string)right;
                return left_final + right_final;
            }

            return null;
        }

        public static object? Substract(object left, object right)
        {
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

            return null;
        }

        public static object? Multiply(object left, object right)
        {
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

            return null;
        }

        public static object? Divide(object left, object right)
        {
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

            return null;
        }

        public static bool And(Interpreter runtime, object left, object right)
        {
            if (left is bool leftBool && right is bool rightBool) return (leftBool && rightBool);

            throw runtime.Error($"Comparison error: cannot compare {left.GetType().Name} with {right.GetType().Name} using And");
        }

        public static bool Or(Interpreter runtime, object left, object right)
        {
            if (left is bool leftBool && right is bool rightBool) return (leftBool || rightBool);

            throw runtime.Error($"Comparison error: cannot compare {left.GetType().Name} with {right.GetType().Name} using Or");
        }

        public static bool Not(Interpreter runtime, object intern)
        {
            if (intern is bool internBool) return !internBool;

            throw runtime.Error($"Type error: NOT expected BOOL but got {intern.GetType().Name}");
        }

        public static bool SameAs(Interpreter runtime, object left, object right)
        {
            if (left is int leftInt && right is int rightInt) return leftInt == rightInt;
            if (left is decimal leftDecimal && right is decimal rightDecimal) return leftDecimal == rightDecimal;
            if (left is int leftIntDecimal && right is decimal rightDecimalInt) return leftIntDecimal == rightDecimalInt;
            if (left is decimal leftDecimalInt && right is int rightIntDecimal) return leftDecimalInt == rightIntDecimal;
            if (left is string leftString && right is string rightString) return leftString == rightString;
            if (left is char leftChar && right is char rightChar) return leftChar == rightChar;
            if (left is bool leftBool && right is bool rightBool) return leftBool == rightBool;

            throw runtime.Error($"Comparison error: cannot compare {left.GetType().Name} with {right.GetType().Name} using SameAs");
        }

        public static bool GreaterThan(Interpreter runtime, object left, object right)
        {
            if (left is int leftInt && right is int rightInt) return leftInt > rightInt;
            if (left is decimal leftDecimal && right is decimal rightDecimal) return leftDecimal > rightDecimal;
            if (left is int leftIntDecimal && right is decimal rightDecimalInt) return leftIntDecimal > rightDecimalInt;
            if (left is decimal leftDecimalInt && right is int rightIntDecimal) return leftDecimalInt > rightIntDecimal;

            throw runtime.Error($"Comparison error: cannot compare {left.GetType().Name} with {right.GetType().Name} using GreaterThan");
        }

        public static bool GreaterOrEqual(Interpreter runtime, object left, object right)
        {
            if (left is int leftInt && right is int rightInt) return leftInt >= rightInt;
            if (left is decimal leftDecimal && right is decimal rightDecimal) return leftDecimal >= rightDecimal;
            if (left is int leftIntDecimal && right is decimal rightDecimalInt) return leftIntDecimal >= rightDecimalInt;
            if (left is decimal leftDecimalInt && right is int rightIntDecimal) return leftDecimalInt >= rightIntDecimal;

            throw runtime.Error($"Comparison error: cannot compare {left.GetType().Name} with {right.GetType().Name} using GreaterOrEqual");
        }

        public static bool LessThan(Interpreter runtime, object left, object right)
        {
            if (left is int leftInt && right is int rightInt) return leftInt < rightInt;
            if (left is decimal leftDecimal && right is decimal rightDecimal) return leftDecimal < rightDecimal;
            if (left is int leftIntDecimal && right is decimal rightDecimalInt) return leftIntDecimal < rightDecimalInt;
            if (left is decimal leftDecimalInt && right is int rightIntDecimal) return leftDecimalInt < rightIntDecimal;

            throw runtime.Error($"Comparison error: cannot compare {left.GetType().Name} with {right.GetType().Name} using LessThan");
        }

        public static bool LessOrEqual(Interpreter runtime, object left, object right)
        {
            if (left is int leftInt && right is int rightInt) return leftInt <= rightInt;
            if (left is decimal leftDecimal && right is decimal rightDecimal) return leftDecimal <= rightDecimal;
            if (left is int leftIntDecimal && right is decimal rightDecimalInt) return leftIntDecimal <= rightDecimalInt;
            if (left is decimal leftDecimalInt && right is int rightIntDecimal) return leftDecimalInt <= rightIntDecimal;

            throw runtime.Error($"Comparison error: cannot compare {left.GetType().Name} with {right.GetType().Name} using LessOrEqual");
        }
    }
}
