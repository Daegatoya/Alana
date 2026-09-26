using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classes.Handler;

namespace Classes.Lexer
{
    public class Lexer
    {
        private Dictionary<string, Type> keywords = new()
        {
            ["define"] = Type.DEFINE,
            ["num"] = Type.NUM,
            ["is"] = Type.EQUAL,
            ["show"] = Type.SHOW,
            ["times"] = Type.MULTI,
            ["divide"] = Type.DIVIDE,
            ["str"] = Type.STR,
            ["end"] = Type.EXITFUNC,
            ["func"] = Type.DEFFUNC,
            ["return"] = Type.RETURN,
            ["call"] = Type.CALL,
            ["decimal"] = Type.DECIMAL,
            ["schar"] = Type.CHAR,
            ["boolean"] = Type.BOOL,
            ["sameas"] = Type.EQUAL_EQUAL,
            ["if"] = Type.IF,
            ["not"] = Type.NOT,
            ["greaterthan"] = Type.GREATERTHAN,
            ["lessthan"] = Type.LESSTHAN,
            ["or"] = Type.OR,
            ["and"] = Type.AND,
            ["grorsame"] = Type.GREATEROREQUAL,
            ["leorsame"] = Type.LESSOREQUAL,
            ["elseif"] = Type.ELSEIF,
            ["else"] = Type.ELSE,
        };
        private string source { get; }
        private int pos = 0;
        private int line = 1, col = 1;
        private string[] sources;

        public Lexer(string source)
        {
            this.source = source;
            sources = source.Split('\n');
        }

        public Token[] TranslateToken()
        {
            List<Token> tokens = new();

            while (pos < source.Length)
            {
                char c = source[pos];

                if (c == '\n')
                {
                    string currentLine = sources[line - 1].TrimEnd('\r', ' ', '\t');
                    if (currentLine.Length > 0 && currentLine[^1] != '#') throw new AlanaError("Expected end of line (#) but couldn't find it", line, currentLine.Length, sources[line - 1]);
                    line++;
                    col = 1;
                    pos++;
                    continue;
                }

                if (char.IsWhiteSpace(c))
                {
                    pos++;
                    col++;
                    continue;
                }

                else if(c == '#')
                {
                    tokens.Add(new Token(Type.END, line, col, sources[line - 1]));
                    pos++;
                    col++;
                    continue;
                }

                else if (c == ',')
                {
                    tokens.Add(new Token(Type.COMA, line, col, sources[line - 1]));
                    pos++;
                    col++;
                    continue;
                }

                else if (c == '(')
                {
                    tokens.Add(new Token(Type.OPEN, line, col, sources[line - 1]));
                    pos++;
                    col++;
                    continue;
                }

                else if (c == ')')
                {
                    tokens.Add(new Token(Type.CLOSE, line, col, sources[line - 1]));
                    pos++;
                    col++;
                    continue;
                }

                else if (char.IsDigit(c))
                {
                    string number = string.Empty;

                    int colBeforeLoop = col;
                    while (pos < source.Length && char.IsDigit(source[pos]))
                    {
                        number += source[pos];
                        pos++;
                        col++;
                    }
                    if (pos < source.Length && source[pos] == '.')
                    {
                        number += source[pos];
                        pos++;
                        col++;
                        while (pos < source.Length && char.IsDigit(source[pos]))
                        {
                            number += source[pos];
                            pos++;
                            col++;
                        }
                        tokens.Add(new Token(Type.DECIMAL_NUM, line, colBeforeLoop, sources[line - 1], number.ToString()));
                        continue;
                    }
                    tokens.Add(new Token(Type.DIGIT, line, colBeforeLoop, sources[line - 1], number.ToString()));
                    continue;
                }

                else if (c == '+')
                {
                    tokens.Add(new Token(Type.ADD, line, col, sources[line - 1]));
                    pos++;
                    col++;
                    continue;
                }

                else if (c == '-')
                {
                    tokens.Add(new Token(Type.SUB, line, col, sources[line - 1]));
                    pos++;
                    col++;
                    continue;
                }

                else if (c == '"')
                {
                    tokens.Add(new Token(Type.OPENCLOSESTR, line, col, sources[line - 1]));
                    pos++;
                    col++;
                    string? word = string.Empty;
                    int colBeforeLoop = col;
                    while (pos < source.Length && source[pos] is not '"')
                    {
                        word += source[pos];
                        pos++;
                        col++;
                    }
                    tokens.Add(new Token(Type.STRING, line, colBeforeLoop, sources[line - 1], word.ToString()));
                    if (pos < source.Length && source[pos] == '"')
                    {
                        tokens.Add(new Token(Type.OPENCLOSESTR, line, col, sources[line - 1]));
                        pos++;
                        col++;
                        continue;
                    }
                    else
                    {
                        throw new AlanaError($"Expected {Type.OPENCLOSESTR} but received {source[pos]}", line, colBeforeLoop, sources[line - 1]);
                    }
                }

                else if (c == '\'')
                {
                    tokens.Add(new Token(Type.OPENCLOSECHAR, line, col, sources[line - 1]));
                    pos++;
                    col++;
                    string? letter = "";
                    int colBeforeLoop = col;
                    while (pos < source.Length && source[pos] is not '\'')
                    {
                        letter += source[pos];
                        pos++;
                        col++;
                    }
                    bool IsChar = char.TryParse(letter, out char result);
                    if (IsChar) tokens.Add(new Token(Type.CHAR, line, colBeforeLoop, sources[line - 1], letter));
                    else throw new AlanaError("SCHAR error: expected a char ('') but received another type", line, colBeforeLoop, sources[line - 1]);
                    if (pos < source.Length && source[pos] == '\'')
                    {
                        tokens.Add(new Token(Type.OPENCLOSECHAR, line, col, sources[line - 1]));
                        pos++;
                        col++;
                        continue;
                    }
                    else
                    {
                        throw new AlanaError($"Expected {Type.OPENCLOSECHAR} but received {source[pos]}", line, colBeforeLoop, sources[line - 1]);
                    }
                }

                else if (char.IsLetter(c))
                {
                    string? word = string.Empty;
                    int colBeforeLoop = col;
                    while (pos < source.Length && char.IsLetterOrDigit(source[pos]))
                    {
                        word += source[pos];
                        pos++;
                        col++;
                    }

                    if (keywords.TryGetValue(word, out Type type))
                    {
                        tokens.Add(new Token(type, line, colBeforeLoop, sources[line - 1]));
                    }
                    else
                    {
                        tokens.Add(new Token(Type.VAR, line, colBeforeLoop, sources[line - 1], word.ToString()));
                    }

                    continue;
                }

                throw new AlanaError($"Unknown character {c}", line, col, sources[line - 1]);
            }

            return tokens.ToArray();
        }
    }
}
