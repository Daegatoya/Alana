using Classes.AST;
using Classes.AST.Expressions;
using Classes.AST.Models;
using Classes.Handler;
using Classes.Lexer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Classes.Parser
{
    public class Parser
    {
        #region Declaration & Parser Base
        private Token[] tokens;
        private int pos = 0;
        private Visibility visibility = Visibility.PUBLIC;

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

        private void SetPos(Statement statement, Token token)
        {
            statement.line = token.line;
            statement.col = token.col;
            statement.source = token.source;
        }

        private void SetPos(Expression expression, Token token)
        {
            expression.line = token.line;
            expression.col = token.col;
            expression.source = token.source;
        }

        public List<Statement> Parse()
        {
            List<Statement> results = new();

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
            Token start = tokens[pos];
            Statement? statement = ParseMatchedStatement();

            if (statement is not null)
            {
                SetPos(statement, start);
            }

            return statement;
        }

        private Statement? ParseMatchedStatement()
        {
            if (tokens[pos].type == Type.CALL)
            {
                Expression call = ParseCall();
                Use(Type.END);

                return new CallStatement(call);
            }
            if (tokens[pos].type == Type.DEFINE && tokens[pos+1].type == Type.NUM)
            {
                return ParseVariable(Type.NUM, VarType.NUM);
            }

            else if (tokens[pos].type == Type.SHOWLN)
            {
                return ParseShowLn();
            }

            else if (tokens[pos].type == Type.DEFINE && tokens[pos + 1].type == Type.VAR && tokens[pos + 2].type == Type.VAR)
            {
                return ParseObjectVariable();
            }

            else if (tokens[pos].type == Type.DEFINE && tokens[pos + 1].type == Type.STR)
            {
                return ParseVariable(Type.STR, VarType.STR);
            }

            else if (tokens[pos].type == Type.PUSH)
            {
                return ParsePush();
            }

            else if (tokens[pos].type == Type.POP)
            {
                return ParsePop();
            }

            else if (tokens[pos].type == Type.TRYPARSENUM || tokens[pos].type == Type.TRYPARSEDEC || tokens[pos].type == Type.TRYPARSEBOOL || tokens[pos].type == Type.TRYPARSECHAR)
            {
                return ParseTryParse();
            }

            else if (tokens[pos].type == Type.DEFINE && tokens[pos + 1].type == Type.BOOL)
            {
                return ParseVariable(Type.BOOL, VarType.BOOL);
            }

            else if (tokens[pos].type == Type.DEFINE && tokens[pos + 1].type == Type.DECIMAL)
            {
                return ParseVariable(Type.DECIMAL, VarType.DECIMAL);
            }

            else if (tokens[pos].type == Type.DEFINE && tokens[pos + 1].type == Type.CHAR)
            {
                return ParseVariable(Type.CHAR, VarType.CHAR);
            }

            else if (tokens[pos].type == Type.DEFINE && tokens[pos + 1].type == Type.DEFFUNC)
            {
                return ParseFunction();
            }

            else if (tokens[pos].type == Type.DEFINE && tokens[pos + 1].type == Type.CLASS)
            {
                return ParseClass();
            }

            else if (tokens[pos].type == Type.REDEFINE)
            {
                return ParseRedefine();
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

            if (tokens[pos].type == Type.READLN || tokens[pos].type == Type.READK)
            {
                return ParseAutonomousStatement();
            }

            if (tokens[pos].type == Type.WHILE)
            {
                return ParseWhile();
            }

            if (tokens[pos].type == Type.FOR)
            {
                return ParseFor();
            }

            if (tokens[pos].type == Type.FOREVERY)
            {
                return ParseForEvery();
            }

            if (tokens[pos].type == Type.CONTINUE)
            {
                return ParseContinue();
            }

            if (tokens[pos].type == Type.BREAK)
            {
                return ParseBreak();
            }

            if(tokens[pos].type == Type.IMPORT)
            {
                return ParseImport();
            }

            if (tokens[pos].type == Type.PUBLIC)
            {
                Use(Type.PUBLIC);
                Use(Type.TWODOTS);
                visibility = Visibility.PUBLIC;
                return null;
            }

            if (tokens[pos].type == Type.PRIVATE)
            {
                Use(Type.PRIVATE);
                Use(Type.TWODOTS);
                visibility = Visibility.PRIVATE;
                return null;
            }

            Token token = tokens[pos];

            throw new AlanaError($"Unexpected token: {token.type} ({token.value})", token.line, token.col, token.source! );
        }
        #endregion

        #region Statement Parsers

        #region Variables Parsers
        private Expression ParseConversion()
        {
            Token conversion = Use(Type.PARSENUM, Type.PARSESTR, Type.PARSEBOOL, Type.PARSECHAR, Type.PARSEDEC);
            Use(Type.OPEN);
            Expression exp = ParseExpression();
            Use(Type.CLOSE);

            Expression parsed = conversion.type switch
            {
                Type.PARSENUM => new ParseNum(exp),
                Type.PARSESTR => new ParseString(exp),
                Type.PARSEBOOL => new ParseBool(exp),
                Type.PARSECHAR => new ParseChar(exp),
                _ => new ParseDecimal(exp)
            };

            SetPos(parsed, conversion);
            return parsed;
        }

        public TryParseStatement ParseTryParse()
        {
            Token conversion = Use(Type.TRYPARSENUM, Type.TRYPARSEDEC, Type.TRYPARSEBOOL, Type.TRYPARSECHAR);
            Use(Type.OPEN);
            Expression value = ParseExpression();
            Use(Type.COMA);
            Token target = Use(Type.VAR);
            Use(Type.COMA);
            Token success = Use(Type.VAR);
            Use(Type.CLOSE);
            Use(Type.END);

            VarType type = conversion.type switch
            {
                Type.TRYPARSENUM => VarType.NUM,
                Type.TRYPARSEDEC => VarType.DECIMAL,
                Type.TRYPARSECHAR => VarType.CHAR,
                Type.TRYPARSEBOOL => VarType.BOOL,

                _ => throw new AlanaError($"Invalid type parsed for tryparse", conversion.line, conversion.col, conversion.source!)
            };

            return new TryParseStatement(value, target.value!, success.value!, type);
        }

        private DefineVar ParseVariable(Type typeToken, VarType type, Type terminator = Type.END)
        {
            Use(Type.DEFINE);
            Use(typeToken);
            bool isArray = ParseArrayType();
            Token name = Use(Type.VAR);
            Use(Type.EQUAL);
            Expression? value = ParseExpression();
            Use(terminator);

            return new DefineVar(visibility, name.value!, type, value, isArray);
        }

        public Statement ParseRedefine(Type terminator = Type.END)
        {
            Use(Type.REDEFINE);

            Expression target = ParseValue();

            if (target is not VarExpression && target is not SelfField && target is not FieldAccess && target is not ArrayAccessExpression)
            {
                Token token = tokens[pos];
                throw new AlanaError("Invalid redefine target", token.line, token.col, token.source!);
            }

            Use(Type.EQUAL);
            Expression value = ParseExpression();
            Use(terminator);

            return new RedefineTarget(target, value);
        }

        private VarType ParseVarType(Token type_Token, string target)
        {
            return type_Token.type switch
            {
                Type.NUM => VarType.NUM,
                Type.STR => VarType.STR,
                Type.CHAR => VarType.CHAR,
                Type.DECIMAL => VarType.DECIMAL,
                Type.BOOL => VarType.BOOL,

                _ => throw new AlanaError($"Invalid type parsed for {target}", type_Token.line, type_Token.col, type_Token.source!)
            };
        }
        #endregion

        #region Return Parsers
        private ContinueStatement ParseContinue()
        {
            Use(Type.CONTINUE);
            Use(Type.END);
            return new ContinueStatement();
        }

        private BreakStatement ParseBreak()
        {
            Use(Type.BREAK);
            Use(Type.END);
            return new BreakStatement();
        }

        public ReturnStatement ParseReturn()
        {
            Use(Type.RETURN);
            Expression? expression = ParseExpression();
            Use(Type.END);
            return new ReturnStatement(expression);
        }
        #endregion

        #region Autonomous & Unary Parser
        private ExpressionStatement ParseAutonomousStatement()
        {
            Expression expr = ParseExpression();
            Use(Type.END);
            return new ExpressionStatement(expr);
        }

        private Expression ParseUnaryExpression()
        {
            if (tokens[pos].type == Type.NOT)
            {
                Token not = Use(Type.NOT);

                NotExpression result = new NotExpression(ParseUnaryExpression());

                SetPos(result, not);

                return result;
            }

            if (tokens[pos].type == Type.SUB)
            {
                Token minus = Use(Type.SUB);

                NegateExpression result = new NegateExpression(ParseUnaryExpression());

                SetPos(result, minus);

                return result;
            }

            return ParseComparisonExpression();
        }
        #endregion

        #region If Statements Parser
        public IfStatement ParseIfStatement()
        {
            Visibility previous = visibility;
            List<Statement> body = new();
            List<ElseIfStatement>? elseIfStatements = null;
            List<Statement>? elseBody = null;
            Use(Type.IF);
            Expression condition = ParseExpression();
            Use(Type.END);
            while (pos < tokens.Length && tokens[pos].type != Type.ELSE && tokens[pos].type != Type.ELSEIF && tokens[pos].type != Type.EXITFUNC)
            {
                Statement? statement = ParseStatement();
                if (statement is not null)
                {
                    body.Add(statement);
                }
            }
            while (pos < tokens.Length && tokens[pos].type == Type.ELSEIF)
            {
                Use(Type.ELSEIF);
                Expression elseIfCondition = ParseExpression();
                Use(Type.END);
                List<Statement> elseIfBody = new();

                while (pos < tokens.Length && tokens[pos].type != Type.ELSE && tokens[pos].type != Type.ELSEIF && tokens[pos].type != Type.EXITFUNC)
                {
                    Statement? eiStatement = ParseStatement();
                    if (eiStatement is not null)
                    {
                        elseIfBody.Add(eiStatement);
                    }
                }

                if (elseIfStatements == null)
                {
                    elseIfStatements = new();
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
                    Statement? eStatement = ParseStatement();
                    if (eStatement is not null)
                    {
                        elseBody.Add(eStatement);
                    }
                }
            }
            Use(Type.EXITFUNC);
            Use(Type.END);
            visibility = previous;
            return new IfStatement(condition, body, elseBody, elseIfStatements);
        }
        #endregion

        #region Loops Parsers
        public WhileStatement ParseWhile()
        {
            Visibility previous = visibility;
            List<Statement> body = new();
            Use(Type.WHILE);
            Expression condition = ParseExpression();
            Use(Type.END);
            while (pos < tokens.Length && tokens[pos].type != Type.EXITFUNC)
            {
                Statement? statement = ParseStatement();
                if (statement is not null)
                {
                    body.Add(statement);
                }
            }
            Use(Type.EXITFUNC);
            Use(Type.END);
            visibility = previous;
            return new WhileStatement(condition, body);
        }

        public ForStatement ParseFor()
        {
            Visibility previous = visibility;
            List<Statement> body = new();
            Use(Type.FOR);
            Use(Type.OPEN);

            Token type_Init = tokens[pos + 1];
            VarType type_Parsed = ParseVarType(type_Init, "the for loop variable");

            DefineVar initialization = ParseVariable(type_Init.type, type_Parsed, Type.COMA);

            Expression condition = ParseExpression();
            Use(Type.COMA);

            Statement step = ParseRedefine(Type.CLOSE);

            Use(Type.END);
            while (pos < tokens.Length && tokens[pos].type != Type.EXITFUNC)
            {
                Statement? statement = ParseStatement();
                if (statement is not null)
                {
                    body.Add(statement);
                }
            }
            Use(Type.EXITFUNC);
            Use(Type.END);
            visibility = previous;
            return new ForStatement(initialization, condition, step, body);
        }

        public ForEveryStatement ParseForEvery()
        {
            Visibility previous = visibility;
            List<Statement> body = new();

            Use(Type.FOREVERY);
            Use(Type.OPEN);
            Use(Type.DEFINE);

            Token type_Element = Use(Type.STR, Type.NUM, Type.DECIMAL, Type.CHAR, Type.BOOL);

            bool isArray = ParseArrayType();

            Token name = Use(Type.VAR);

            VarType type_Parsed = ParseVarType(type_Element, $"the forevery variable {name.value}");

            Use(Type.WITHIN);

            Expression target = ParsePostfixExpression();

            Use(Type.CLOSE);
            Use(Type.END);

            while (pos < tokens.Length && tokens[pos].type != Type.EXITFUNC)
            {
                Statement? statement = ParseStatement();

                if (statement is not null)
                {
                    body.Add(statement);
                }
            }

            Use(Type.EXITFUNC);
            Use(Type.END);

            visibility = previous;

            return new ForEveryStatement(name.value!, type_Parsed, isArray, target, body);
        }
        #endregion

        #region Parse Call

        public Expression ParseCall()
        {
            Token callToken = Use(Type.CALL);
            Token first = Use(Type.VAR, Type.SELF);

            if (first.type == Type.VAR && tokens[pos].type == Type.OPEN)
            {
                Use(Type.OPEN);

                List<Expression> arguments = new();

                while (tokens[pos].type != Type.CLOSE)
                {
                    arguments.Add(ParseExpression());

                    if (tokens[pos].type != Type.CLOSE)
                    {
                        Use(Type.COMA);
                    }
                }

                Use(Type.CLOSE);

                FuncCall functionCall = new FuncCall(first.value!, arguments);
                SetPos(functionCall, callToken);
                return functionCall;
            }

            Expression target;

            if (first.type == Type.SELF)
            {
                target = new VarExpression("self");
            }
            else
            {
                target = new VarExpression(first.value!);
            }

            SetPos(target, first);

            while (tokens[pos].type == Type.DOT)
            {
                Token dot = Use(Type.DOT);
                Token member = Use(Type.VAR);

                if (tokens[pos].type == Type.OPEN)
                {
                    Use(Type.OPEN);

                    List<Expression> arguments = new();

                    while (tokens[pos].type != Type.CLOSE)
                    {
                        arguments.Add(ParseExpression());

                        if (tokens[pos].type != Type.CLOSE)
                        {
                            Use(Type.COMA);
                        }
                    }

                    Use(Type.CLOSE);

                    MethodCall methodCall = new MethodCall(target, member.value!, arguments);
                    SetPos(methodCall, dot);
                    target = methodCall;
                }

                else
                {
                    FieldAccess fieldAccess = new FieldAccess(target, member.value!);
                    SetPos(fieldAccess, dot);
                    target = fieldAccess;
                }
            }

            if (target is not MethodCall)
            {
                throw new AlanaError("Expected method call", callToken.line, callToken.col, callToken.source!);
            }

            return target;
        }

        #endregion

        #region Functions Parsers
        public DefFunction ParseFunction()
        {
            string? returnClassName = null;
            Visibility functionVisibility = visibility;
            Visibility previous = visibility;
            List<Statement> body = new();
            List<Param> @params = new();
            VarType? returnType = null;
            bool returnIsArray = false;

            Use(Type.DEFINE);
            Use(Type.DEFFUNC);

            Token name = Use(Type.VAR);

            Use(Type.OPEN);

            while (pos < tokens.Length && tokens[pos].type != Type.CLOSE)
            {
                Token type_Param = Use(Type.STR, Type.NUM, Type.DECIMAL, Type.CHAR, Type.BOOL, Type.VAR);

                bool isArray = ParseArrayType();
                Token name_Param = Use(Type.VAR);

                if (type_Param.type == Type.VAR)
                {
                    @params.Add(new Param(name_Param.value!, type_Param.value!, isArray));
                }
                else
                {
                    VarType type_Parsed = ParseVarType(type_Param, $"parameter {name_Param.value}");
                    @params.Add(new Param(name_Param.value!, type_Parsed, isArray));
                }

                if (tokens[pos].type != Type.CLOSE)
                {
                    Use(Type.COMA);
                }
            }

            Use(Type.CLOSE);
            if (tokens[pos].type == Type.RETURN_TYPE)
            {
                Use(Type.RETURN_TYPE);

                Token type = Use(Type.BOOL, Type.STR, Type.CHAR, Type.DECIMAL, Type.NUM, Type.VAR);

                if (type.type == Type.VAR)
                {
                    returnClassName = type.value!;
                }
                else
                {
                    returnType = ParseVarType(type, name.value!);
                    returnIsArray = ParseArrayType();
                }
            }
            Use(Type.END);

            while (pos < tokens.Length && tokens[pos].type != Type.EXITFUNC)
            {
                Statement? statement = ParseStatement();
                if (statement is not null)
                {
                    body.Add(statement);
                }
            }

            Use(Type.EXITFUNC);
            Use(Type.END);

            visibility = previous;
            return new DefFunction(functionVisibility, name.value!, @params, body, returnType, returnClassName, returnIsArray);
        }
        #endregion

        #region Class Parser

        private DefineObject ParseObjectVariable()
        {
            Use(Type.DEFINE);

            Token className = Use(Type.VAR);
            Token variableName = Use(Type.VAR);

            Use(Type.EQUAL);

            Expression value = ParseExpression();

            Use(Type.END);

            return new DefineObject(visibility, className.value!, variableName.value!, value);
        }

        private NewObject ParseNewObject()
        {
            Token start = Use(Type.NEWOBJ);

            Token className = Use(Type.VAR);

            Use(Type.OPEN);

            List<Expression> arguments = new();

            while (tokens[pos].type != Type.CLOSE)
            {
                arguments.Add(ParseExpression());

                if (tokens[pos].type != Type.CLOSE)
                {
                    Use(Type.COMA);
                }
            }

            Use(Type.CLOSE);

            NewObject result = new(className.value!, arguments);

            SetPos(result, start);

            return result;
        }

        public DefClass ParseClass()
        {
            Visibility classVisibility = visibility;
            Visibility previous = visibility;

            List<DefineVar> fields = new();
            List<DefineObject> objectFields = new();
            List<DefFunction> methods = new();
            DefConstructor? constructor = null;

            Use(Type.DEFINE);
            Use(Type.CLASS);

            Token name = Use(Type.VAR);
            Use(Type.END);

            visibility = Visibility.PRIVATE;

            while (pos < tokens.Length && tokens[pos].type != Type.EXITFUNC)
            {
                if (tokens[pos].type == Type.PUBLIC)
                {
                    Use(Type.PUBLIC);
                    Use(Type.TWODOTS);
                    visibility = Visibility.PUBLIC;
                    continue;
                }

                if (tokens[pos].type == Type.PRIVATE)
                {
                    Use(Type.PRIVATE);
                    Use(Type.TWODOTS);
                    visibility = Visibility.PRIVATE;
                    continue;
                }

                if (tokens[pos].type == Type.DEFINE && tokens[pos + 1].type == Type.DEFFUNC)
                {
                    methods.Add(ParseFunction());
                    continue;
                }

                if (tokens[pos].type == Type.DEFINE && tokens[pos + 1].type == Type.CONSTRUCTOR)
                {
                    if (constructor is not null)
                    {
                        throw new AlanaError($"Class {name.value} already has a constructor", tokens[pos].line, tokens[pos].col, tokens[pos].source!);
                    }

                    constructor = ParseConstructor();
                    continue;
                }

                if (tokens[pos].type == Type.DEFINE)
                {
                    Token typeToken = tokens[pos + 1];

                    if (typeToken.type == Type.NUM)
                    {
                        fields.Add(ParseVariable(Type.NUM, VarType.NUM));
                        continue;
                    }

                    if (typeToken.type == Type.STR)
                    {
                        fields.Add(ParseVariable(Type.STR, VarType.STR));
                        continue;
                    }

                    if (typeToken.type == Type.DECIMAL)
                    {
                        fields.Add(ParseVariable(Type.DECIMAL, VarType.DECIMAL));
                        continue;
                    }

                    if (typeToken.type == Type.CHAR)
                    {
                        fields.Add(ParseVariable(Type.CHAR, VarType.CHAR));
                        continue;
                    }

                    if (typeToken.type == Type.BOOL)
                    {
                        fields.Add(ParseVariable(Type.BOOL, VarType.BOOL));
                        continue;
                    }

                    if (typeToken.type == Type.VAR)
                    {
                        objectFields.Add(ParseObjectVariable());
                        continue;
                    }
                }

                Token token = tokens[pos];

                throw new AlanaError($"Invalid class member: {token.type}", token.line, token.col, token.source!);
            }

            Use(Type.EXITFUNC);
            Use(Type.END);

            visibility = previous;
            return new DefClass(classVisibility, name.value!, fields, objectFields, methods, constructor);
        }

        public DefConstructor ParseConstructor()
        {
            Visibility constructorVisibility = visibility;
            Visibility previous = visibility;

            List<Statement> body = new();
            List<Param> @params = new();

            Use(Type.DEFINE);
            Use(Type.CONSTRUCTOR);

            Use(Type.OPEN);

            while (pos < tokens.Length && tokens[pos].type != Type.CLOSE)
            {
                Token type_Param = Use(Type.STR, Type.NUM, Type.DECIMAL, Type.CHAR, Type.BOOL, Type.VAR);

                bool isArray = ParseArrayType();
                Token name_Param = Use(Type.VAR);

                if (type_Param.type == Type.VAR)
                {
                    @params.Add(new Param(name_Param.value!, type_Param.value!, isArray));
                }
                else
                {
                    VarType type_Parsed = ParseVarType(type_Param, $"constructor parameter {name_Param.value}");
                    @params.Add(new Param(name_Param.value!, type_Parsed, isArray));
                }

                if (tokens[pos].type != Type.CLOSE)
                {
                    Use(Type.COMA);
                }
            }

            Use(Type.CLOSE);
            Use(Type.END);

            while (pos < tokens.Length && tokens[pos].type != Type.EXITFUNC)
            {
                Statement? statement = ParseStatement();

                if (statement is not null)
                {
                    body.Add(statement);
                }
            }

            Use(Type.EXITFUNC);
            Use(Type.END);

            visibility = previous;

            return new DefConstructor(constructorVisibility, @params, body);
        }

        #endregion

        #region Show/ShowLN Parsers
        public Statement ParseShow()
        {
            Use(Type.SHOW);
            if (pos < tokens.Length && tokens[pos].type == Type.END)
            {
                Use(Type.END);
                return new Show();
            }
            Expression? expression = ParseExpression();
            Use(Type.END);

            return new Show(expression);
        }

        public Statement ParseShowLn()
        {
            Use(Type.SHOWLN);
            if (pos < tokens.Length && tokens[pos].type == Type.END)
            {
                Use(Type.END);
                return new ShowLn();
            }
            Expression? expression = ParseExpression();
            Use(Type.END);

            return new ShowLn(expression);
        }
        #endregion

        #region Array Parsers
        public ArrayExpression ParseArrayExpression()
        {
            List<Expression> values = new();
            Token open = Use(Type.OPENARRAY);
            while (pos < tokens.Length && tokens[pos].type != Type.CLOSEARRAY)
            {
                Expression value = ParseExpression();
                values.Add(value);

                if (tokens[pos].type != Type.CLOSEARRAY)
                {
                    Use(Type.COMA);
                }
            }

            Use(Type.CLOSEARRAY);
            ArrayExpression result = new ArrayExpression(values);
            SetPos(result, open);
            return result;
        }

        public bool ParseArrayType()
        {
            if (tokens[pos].type == Type.OPENARRAY)
            {
                Use(Type.OPENARRAY);
                Use(Type.CLOSEARRAY);
                return true;
            }
            return false;
        }

        public PushStatement ParsePush()
        {
            Use(Type.PUSH);
            Expression target = ParsePostfixExpression();
            Use(Type.WITH);
            Expression value = ParseExpression();
            Use(Type.END);

            return new PushStatement(target, value);
        }

        public PopStatement ParsePop()
        {
            Use(Type.POP);
            Expression target = ParsePostfixExpression();
            Use(Type.END);

            return new PopStatement(target);
        }
        #endregion

        #region Import Parser

        private ImportStatement ParseImport()
        {
            Use(Type.IMPORT);
            Use(Type.OPENCLOSESTR);
            Token file = Use(Type.STRING);
            Use(Type.OPENCLOSESTR);
            Use(Type.END);

            return new ImportStatement(file.value!);
        }

        #endregion

        #endregion

        #region Value Parsers
        private Expression ParseValue()
        {
            return ParsePostfixExpression();
        }

        private Expression ParsePostfixExpression()
        {
            Expression expression = ParsePrimary();

            while (pos < tokens.Length)
            {
                if (tokens[pos].type == Type.OPENARRAY)
                {
                    Token open = Use(Type.OPENARRAY);
                    Expression index = ParseExpression();
                    Use(Type.CLOSEARRAY);
                    ArrayAccessExpression access = new ArrayAccessExpression(expression, index);

                    SetPos(access, open);
                    expression = access;
                    continue;
                }

                if (tokens[pos].type == Type.DOT)
                {
                    Token dot = Use(Type.DOT);
                    Token fieldName = Use(Type.VAR);
                    FieldAccess access = new FieldAccess(expression, fieldName.value!);

                    SetPos(access, dot);
                    expression = access;
                    continue;
                }

                break;
            }

            return expression;
        }

        private Expression ParsePrimary()
        {
            if (tokens[pos].type == Type.SELF)
            {
                return ParseSelfField();
            }

            if (tokens[pos].type == Type.NEWOBJ)
            {
                return ParseNewObject();
            }

            if (tokens[pos].type == Type.READLN)
            {
                Token readln = Use(Type.READLN);
                ReadLine result = new ReadLine();

                SetPos(result, readln);
                return result;
            }

            if (tokens[pos].type == Type.READK)
            {
                Token readk = Use(Type.READK);
                ReadKey result = new ReadKey();

                SetPos(result, readk);
                return result;
            }

            if (tokens[pos].type == Type.LENGTH)
            {
                Token length = Use(Type.LENGTH);
                Expression target = ParseValue();
                Length result = new Length(target);

                SetPos(result, length);
                return result;
            }

            if (tokens[pos].type == Type.OPENCLOSESTR)
            {
                Token open = Use(Type.OPENCLOSESTR);
                Token value = Use(Type.STRING);
                Use(Type.OPENCLOSESTR);
                StringExpression result = new StringExpression(value.value!);

                SetPos(result, open);
                return result;
            }

            if (tokens[pos].type == Type.OPENCLOSECHAR)
            {
                Token open = Use(Type.OPENCLOSECHAR);
                Token value = Use(Type.CHAR);
                Use(Type.OPENCLOSECHAR);
                CharExpression result = new CharExpression(char.Parse(value.value!));

                SetPos(result, open);
                return result;
            }

            if (tokens[pos].type == Type.OPENARRAY)
            {
                return ParseArrayExpression();
            }

            if (tokens[pos].type == Type.CALL)
            {
                return ParseCall();
            }

            if (tokens[pos].type == Type.PARSEBOOL || tokens[pos].type == Type.PARSENUM || tokens[pos].type == Type.PARSESTR || tokens[pos].type == Type.PARSEDEC || tokens[pos].type == Type.PARSECHAR)
            {
                return ParseConversion();
            }

            Token token = Use(Type.VAR, Type.DIGIT, Type.DECIMAL_NUM, Type.NEW);

            if (token.type == Type.VAR)
            {
                if (bool.TryParse(token.value!, out bool boolValue))
                {
                    BoolExpression boolean = new BoolExpression(boolValue);

                    SetPos(boolean, token);
                    return boolean;
                }

                VarExpression variable = new VarExpression(token.value!);

                SetPos(variable, token);
                return variable;
            }

            if (token.type == Type.DECIMAL_NUM)
            {
                DecimalExpression decimalValue = new DecimalExpression(decimal.Parse(token.value!, CultureInfo.InvariantCulture));

                SetPos(decimalValue, token);
                return decimalValue;
            }

            if (token.type == Type.NEW)
            {
                NewExpression newExpression = new NewExpression();

                SetPos(newExpression, token);
                return newExpression;
            }

            NumberExpression number = new NumberExpression(int.Parse(token.value!));

            SetPos(number, token);
            return number;
        }

        private Expression ParseComparisonExpression()
        {
            Expression left = ParseValue();

            while (tokens[pos].type != Type.END && tokens[pos].type != Type.COMA && tokens[pos].type != Type.CLOSE && tokens[pos].type != Type.AND && tokens[pos].type != Type.OR && tokens[pos].type != Type.CLOSEARRAY)
            {
                Token opToken = tokens[pos];
                Type operation = opToken.type;
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

                    case Type.MODULO:
                        left = new Modulo(left, right);
                        break;

                    default:
                        Token token = tokens[pos];
                        throw new AlanaError($"Unexpected operator: {operation}", token.line, token.col, token.source!);
                }

                SetPos(left, opToken);
            }

            return left;
        }

        private Expression ParseExpression()
        {
            Expression left = ParseUnaryExpression();

            while (tokens[pos].type == Type.AND || tokens[pos].type == Type.OR)
            {
                Token opToken = tokens[pos];
                Type operation = opToken.type;
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

                SetPos(left, opToken);
            }

            return left;
        }

        private SelfField ParseSelfField()
        {
            Token self = Use(Type.SELF);

            Use(Type.DOT);

            Token field = Use(Type.VAR);

            SelfField result = new SelfField(field.value!);

            SetPos(result, self);

            return result;
        }
        #endregion
    }
}


