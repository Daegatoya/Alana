using Classes.AST;
using Classes.AST.Expressions;
using Classes.AST.Models;
using Classes.Lexer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classes.Handler;

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
                throw new AlanaError("Unexpected end of file", tokens[^1].line, tokens[^1].col, tokens[^1].source!);
            }

            Token token = tokens[pos];

            if (!expectedTypes.Contains(token.type))
            {
                throw new AlanaError($"Expected {string.Join(" or ", expectedTypes)} but received {token.type}", token.line, token.col, token.source!);
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
            if (tokens[pos].type == Type.CALL)
            {
                FuncCall call = ParseFuncCall();
                Use(Type.END);
                return new CallStatement(call);
            }
            if (tokens[pos].type == Type.DEFINE && tokens[pos+1].type == Type.NUM)
            {
                return ParseNumVariable();
            }

            else if(tokens[pos].type == Type.DEFINE && tokens[pos+1].type == Type.STR)
            {
                return ParseStrVariable();
            }

            else if (tokens[pos].type == Type.DEFINE && tokens[pos + 1].type == Type.BOOL)
            {
                return ParseBoolVariable();
            }

            else if (tokens[pos].type == Type.DEFINE && tokens[pos + 1].type == Type.DECIMAL)
            {
                return ParseDecimalVariable();
            }

            else if (tokens[pos].type == Type.DEFINE && tokens[pos + 1].type == Type.CHAR)
            {
                return ParseCharVariable();
            }

            else if (tokens[pos].type == Type.DEFINE && tokens[pos + 1].type == Type.DEFFUNC)
            {
                return ParseFunction();
            }

            if (tokens[pos].type == Type.SHOW)
            {
                return ParseShow();
            }

            if (tokens[pos].type == Type.RETURN)
            {
                return ParseReturn();
            }

            if (tokens[pos].type == Type.IF)
            {
                return ParseIfStatement();
            }
            throw new Exception($"Unexpected token: {tokens[pos].type}");
        }

        private Expression ParseUnaryExpression()
        {
            if (tokens[pos].type == Type.NOT)
            {
                Use(Type.NOT);
                return new NotExpression(ParseUnaryExpression());
            }

            return ParseComparisonExpression();
        }

        public IfStatement ParseIfStatement()
        {
            List<Statement> body = new();
            List<ElseIfStatement>? elseIfStatements = null;
            List<Statement>? elseBody = null;
            Use(Type.IF);
            Expression condition = ParseExpression();
            Use(Type.END);
            while (pos < tokens.Length && tokens[pos].type != Type.ELSE && tokens[pos].type != Type.ELSEIF && tokens[pos].type != Type.EXITFUNC)
            {
                Statement statement = ParseStatement()!;
                body.Add(statement);
            }
            if(pos < tokens.Length && tokens[pos].type == Type.ELSEIF) elseIfStatements = new();
            while (pos < tokens.Length && tokens[pos].type == Type.ELSEIF)
            {
                Use(Type.ELSEIF);
                Expression elseIfCondition = ParseExpression();
                Use(Type.END);
                List<Statement> elseIfBody = new();

                while (pos < tokens.Length && tokens[pos].type != Type.ELSE && tokens[pos].type != Type.ELSEIF && tokens[pos].type != Type.EXITFUNC)
                {
                    Statement eiStatement = ParseStatement()!;
                    elseIfBody.Add(eiStatement);
                }

                elseIfStatements.Add(new ElseIfStatement(elseIfCondition, elseIfBody));
            }
            if(pos < tokens.Length && tokens[pos].type == Type.ELSE)
            {
                Use(Type.ELSE);
                Use(Type.END);
                elseBody = new();
                while (pos < tokens.Length && tokens[pos].type != Type.EXITFUNC)
                {
                    Statement eStatement = ParseStatement()!;
                    elseBody!.Add(eStatement);
                }
            }
            Use(Type.EXITFUNC);
            Use(Type.END);
            return new IfStatement(condition, body, elseBody, elseIfStatements);
        }

        public FuncCall ParseFuncCall()
        {
            List<Expression> expressions = new();

            Use(Type.CALL);
            Token name = Use(Type.VAR);
            Use(Type.OPEN);

            while (tokens[pos].type != Type.CLOSE)
            {
                expressions.Add(ParseExpression());

                if (tokens[pos].type != Type.CLOSE)
                {
                    Use(Type.COMA);
                }
            }

            Use(Type.CLOSE);

            return new FuncCall(name.value!, expressions);
        }
        public ReturnStatement ParseReturn()
        {
            Use(Type.RETURN);
            Expression? expression = ParseExpression();
            Use(Type.END);
            return new ReturnStatement(expression);

        }
        public DefineVar ParseNumVariable()
        {
            Use(Type.DEFINE);
            Use(Type.NUM);
            Token name = Use(Type.VAR);
            Use(Type.EQUAL);
            Expression value = ParseExpression();
            Use(Type.END);

            return new DefineVar(name.value!, VarType.NUM, value);
        }

        public DefineVar ParseDecimalVariable()
        {
            Use(Type.DEFINE);
            Use(Type.DECIMAL);
            Token name = Use(Type.VAR);
            Use(Type.EQUAL);
            Expression value = ParseExpression();
            Use(Type.END);

            return new DefineVar(name.value!, VarType.DECIMAL, value);
        }

        public DefineVar ParseBoolVariable()
        {
            Use(Type.DEFINE);
            Use(Type.BOOL);
            Token name = Use(Type.VAR);
            Use(Type.EQUAL);
            Expression value = ParseExpression();
            Use(Type.END);

            return new DefineVar(name.value!, VarType.BOOL, value);
        }

        public DefineVar ParseCharVariable()
        {
            Use(Type.DEFINE);
            Use(Type.CHAR);
            Token name = Use(Type.VAR);
            Use(Type.EQUAL);
            Expression value = ParseExpression();
            Use(Type.END);

            return new DefineVar(name.value!, VarType.CHAR, value);
        }

        public DefineVar ParseStrVariable()
        {
            Use(Type.DEFINE);
            Use(Type.STR);
            Token name = Use(Type.VAR);
            Use(Type.EQUAL);
            Expression value = ParseExpression();
            Use(Type.END);

            return new DefineVar(name.value!, VarType.STR, value);
        }

        public DefFunction ParseFunction()
        {
            List<Statement> body = new();
            List<Param> @params = new();
            Use(Type.DEFINE);
            Use(Type.DEFFUNC);
            Token name = Use(Type.VAR);
            Use(Type.OPEN);
            while (pos < tokens.Length && tokens[pos].type != Type.CLOSE)
            {
                Token type_Param = Use(Type.STR, Type.NUM, Type.DECIMAL, Type.CHAR, Type.BOOL);
                Token name_Param = Use(Type.VAR);
                VarType type_Parsed = type_Param.type switch
                {
                    Type.NUM => VarType.NUM,
                    Type.STR => VarType.STR,
                    Type.CHAR => VarType.CHAR,
                    Type.DECIMAL => VarType.DECIMAL,
                    Type.BOOL => VarType.BOOL,
                    _ => throw new Exception($"Invalid type parsed for parameter {name_Param.value}")
                };
                @params.Add(new Param(name_Param.value!, type_Parsed!));

                if (tokens[pos].type != Type.CLOSE)
                {
                    Use(Type.COMA);
                }
            }
            Use(Type.CLOSE);
            while(pos < tokens.Length && tokens[pos].type != Type.EXITFUNC)
            {
                Statement statement = ParseStatement()!;
                body.Add(statement);
            }
            Use(Type.EXITFUNC);
            Use(Type.END);

            return new DefFunction(name.value!, @params, body);
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
            if (tokens[pos].type == Type.OPENCLOSESTR)
            {
                Use(Type.OPENCLOSESTR);
                Token value = Use(Type.STRING);
                Use(Type.OPENCLOSESTR);

                return new StringExpression(value.value!);
            }

            if (tokens[pos].type == Type.OPENCLOSECHAR)
            {
                Use(Type.OPENCLOSECHAR);
                Token value = Use(Type.CHAR);
                Use(Type.OPENCLOSECHAR);

                return new CharExpression(char.Parse(value.value!));
            }

            if (tokens[pos].type == Type.CALL)
            {
                return ParseFuncCall();
            }

            Token token = Use(Type.VAR, Type.DIGIT, Type.DECIMAL_NUM);

            if (token.type == Type.VAR)
            {
                if (bool.TryParse(token.value!, out bool boolValue))
                {
                    return new BoolExpression(boolValue);
                }

                return new VarExpression(token.value!);
            }

            if (token.type == Type.DECIMAL_NUM)
            {
                return new DecimalExpression(decimal.Parse(token.value!, CultureInfo.InvariantCulture));
            }

            return new NumberExpression(int.Parse(token.value!));
        }

        private Expression ParseComparisonExpression()
        {
            Expression left = ParseValue();

            while (
                tokens[pos].type != Type.END &&
                tokens[pos].type != Type.COMA &&
                tokens[pos].type != Type.CLOSE &&
                tokens[pos].type != Type.AND &&
                tokens[pos].type != Type.OR
            )
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

                    case Type.EQUAL_EQUAL:
                        left = new SameAs(left, right);
                        break;

                    case Type.GREATERTHAN:
                        left = new GreaterThan(left, right);
                        break;

                    case Type.LESSOREQUAL:
                        left = new LessOrEqual(left, right);
                        break;

                    case Type.GREATEROREQUAL:
                        left = new GreaterOrEqual(left, right);
                        break;

                    case Type.LESSTHAN:
                        left = new LessThan(left, right);
                        break;

                    default:
                        throw new Exception($"Unexpected operator: {operation}");
                }
            }

            return left;
        }

        private Expression ParseExpression()
        {
            Expression left = ParseUnaryExpression();

            while (tokens[pos].type == Type.AND || tokens[pos].type == Type.OR)
            {
                Type operation = tokens[pos].type;
                Use(operation);

                Expression right = ParseUnaryExpression();

                if (operation == Type.AND)
                {
                    left = new AndExpression(left, right);
                }
                else if (operation == Type.OR)
                {
                    left = new OrExpression(left, right);
                }
            }

            return left;
        }
    }
}


