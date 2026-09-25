using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                if (source[^1] != '#' && source[^1] != ')') throw new Exception("End of line (#) required.");
                char c = source[pos];

                if(c == '\n')
                {
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
                    try
                    {
                        int colBeforeLoop = col;
                        while (pos < source.Length && source[pos] is not '"')
                        {
                            word += source[pos];
                            pos++;
                            col++;
                        }
                        tokens.Add(new Token(Type.STRING, line, colBeforeLoop, sources[line - 1], word.ToString()));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    if (pos < source.Length && source[pos] == '"')
                    {
                        tokens.Add(new Token(Type.OPENCLOSESTR, line, col, sources[line - 1]));
                        pos++;
                        col++;
                        continue;
                    }
                    else
                    {
                        throw new Exception($"Expected {Type.OPENCLOSESTR} but received {source[pos]}");
                    }
                }

                else if (c == '\'')
                {
                    tokens.Add(new Token(Type.OPENCLOSECHAR, line, col, sources[line - 1]));
                    pos++;
                    col++;
                    string? letter = "";
                    try
                    {
                        int colBeforeLoop = col;
                        while (pos < source.Length && source[pos] is not '\'')
                        {
                            letter += source[pos];
                            pos++;
                            col++;
                        }
                        bool IsChar = char.TryParse(letter, out char result);
                        if (IsChar) tokens.Add(new Token(Type.CHAR, line, colBeforeLoop, sources[line - 1], letter));
                        else throw new Exception("SCHAR error: expected a char ('') but received another type");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    if (pos < source.Length && source[pos] == '\'')
                    {
                        tokens.Add(new Token(Type.OPENCLOSECHAR, line, col, sources[line - 1]));
                        pos++;
                        col++;
                        continue;
                    }
                    else
                    {
                        throw new Exception($"Expected {Type.OPENCLOSECHAR} but received {source[pos]}");
                    }
                }

                else if (char.IsLetter(c))
                {
                    string? word = string.Empty;
                    try
                    {
                        int colBeforeLoop = col;
                        while (pos < source.Length && char.IsLetterOrDigit(source[pos]))
                        {
                            word += source[pos];
                            pos++;
                            col++;
                        }

                        if (keywords.TryGetValue(word, out Type type)) {
                            tokens.Add(new Token(type, line, colBeforeLoop, sources[line - 1]));
                        }
                        else
                        {
                            tokens.Add(new Token(Type.VAR, line, colBeforeLoop, sources[line - 1], word.ToString()));
                        }

                        continue;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                throw new Exception($"Unknown character {c}");
            }

            return tokens.ToArray();
        }
    }
}
