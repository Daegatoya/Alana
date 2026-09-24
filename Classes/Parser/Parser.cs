using Classes.AST;
using Classes.AST.Expressions;
using Classes.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Parser
{
    public class Parser
    {
        private Token[] tokens;
        private int pos = 0;

        public Parser(Token[] tokens)
        {
            this.tokens = tokens;
        }

        private Token Use(params Type[] expectedTypes)
        {
            if (pos >= tokens.Length)
            {
                throw new Exception("Unexpected end of file");
            }

            Token token = tokens[pos];

            if (!expectedTypes.Contains(token.type))
            {
                throw new Exception(
                    $"Expected {string.Join(" or ", expectedTypes)} but received {token.type}"
                );
            }

            pos++;
            return token;
        }

        public List<Statement>? Parse()
        {
            List<Statement>? results = new();

            while(pos < tokens.Length)
            {
                Statement? statement = ParseStatement();

                if(statement is not null)
                {
                    results.Add(statement);
                }
            }

            return results;
        }

        public Statement? ParseStatement()
        {
            if (tokens[pos].type == Type.DEFINE && tokens[pos+1].type == Type.NUM)
            {
                return ParseNumVariable();
            }

            else if(tokens[pos].type == Type.DEFINE && tokens[pos+1].type == Type.STR)
            {
                return ParseStrVariable();
            }

            if (tokens[pos].type == Type.SHOW)
            {
                return ParseShow();
            }

            throw new Exception($"Unexpected token: {tokens[pos].type}");
        }
        public DefineVar ParseNumVariable()
        {
            Use(Type.DEFINE);
            Use(Type.NUM);
            Token name = Use(Type.VAR);
            Use(Type.EQUAL);
            Token value = Use(Type.DIGIT);
            Use(Type.END);

            return new DefineVar(name.value!, VarType.NUM, int.Parse(value.value!));
        }

        public DefineVar ParseStrVariable()
        {
            Use(Type.DEFINE);
            Use(Type.STR);
            Token name = Use(Type.VAR);
            Use(Type.EQUAL);
            Token value = Use(Type.VAR);
            Use(Type.END);

            return new DefineVar(name.value!, VarType.STR, value.value!);
        }

        public Statement ParseShow()
        {
            Use(Type.SHOW);
            Expression expression = ParseExpression();
            Use(Type.END);

            return new Show(expression);
        }

        private Expression ParseValue()
        {
            Token token = Use(Type.VAR, Type.DIGIT);

            if (token.type == Type.VAR)
            {
                return new VarExpression(token.value!);
            }

            return new NumberExpression(int.Parse(token.value!));
        }

        private Expression ParseExpression()
        {
            Expression left = ParseValue();

            while (tokens[pos].type != Type.END)
            {
                Type operation = tokens[pos].type;
                Use(operation);
                Expression right = ParseValue();

                switch (operation)
                {
                    case Type.ADD:
                        left = new Addition(left, right);
                        break;

                    case Type.SUB:
                        left = new Substraction(left, right);
                        break;

                    case Type.MULTI:
                        left = new Multiplication(left, right);
                        break;

                    case Type.DIVIDE:
                        left = new Division(left, right);
                        break;

                    default:
                        throw new Exception($"Unexpected operator: {operation}");
                }
            }

            return left;
        }
    }
}


