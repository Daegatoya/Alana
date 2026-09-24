using Classes.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classes.AST.Expressions;

namespace Classes.Interpreter
{
    public class Interpreter
    {
        private Dictionary<string, object> variables = new();

        public Interpreter() { }

        public void Execute(List<Statement> statements)
        {
            foreach (Statement s in statements)
            {
                if (s is DefineVar d)
                {
                    variables.Add(d.name, d.value);
                }
                else if (s is Show sh)
                {
                    object result = Evaluate(sh.expression);
                    Console.WriteLine(result);
                }
            }
        }

        private object Evaluate(Expression expression)
        {
            if(expression is StringExpression str)
            {
                return str.value;
            }

            if (expression is NumberExpression number)
            {
                return number.value;
            }

            if (expression is VarExpression variable)
            {
                if (!variables.Any(v => v.Key == variable.name)) throw new Exception($"Unknown variable {variable.name}");
                return variables[variable.name];
            }

            if (expression is Addition addition)
            {
                int left = (int)Evaluate(addition.left);
                int right = (int)Evaluate(addition.right);

                return left + right;
            }
            else if (expression is Substraction substraction)
            {
                int left = (int)Evaluate(substraction.left);
                int right = (int)Evaluate(substraction.right);

                return left - right;
            }
            else if (expression is Multiplication multiplication)
            {
                int left = (int)Evaluate(multiplication.left);
                int right = (int)Evaluate(multiplication.right);

                return left * right;
            }
            else if (expression is Division division)
            {
                int left = (int)Evaluate(division.left);
                int right = (int)Evaluate(division.right);

                return left / right;
            }

            throw new Exception("Unknown expression");
        }
    }
}
