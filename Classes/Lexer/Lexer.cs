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

        public Lexer(string source)
        {
            this.source = source;
        }

        public Token[] TranslateToken()
        {
            List<Token> tokens = new();

            while (pos < source.Length)
            {
                if (source[^1] != '#' && source[^1] != ')') throw new Exception("End of line (#) required.");
                char c = source[pos];

                if (char.IsWhiteSpace(c))
                {
                    pos++;
                    continue;
                }

                else if(c == '#')
                {
                    tokens.Add(new Token(Type.END));
                    pos++;
                    continue;
                }

                else if (c == ',')
                {
                    tokens.Add(new Token(Type.COMA));
                    pos++;
                    continue;
                }

                else if (c == '(')
                {
                    tokens.Add(new Token(Type.OPEN));
                    pos++;
                    continue;
                }

                else if (c == ')')
                {
                    tokens.Add(new Token(Type.CLOSE));
                    pos++;
                    continue;
                }

                else if (char.IsDigit(c))
                {
                    string number = string.Empty;

                    while (pos < source.Length && char.IsDigit(source[pos]))
                    {
                        number += source[pos];
                        pos++;
                    }
                    if (pos < source.Length && source[pos] == '.')
                    {
                        number += source[pos];
                        pos++;
                        while (pos < source.Length && char.IsDigit(source[pos]))
                        {
                            number += source[pos];
                            pos++;
                        }
                        tokens.Add(new Token(Type.DECIMAL_NUM, number.ToString()));
                        continue;
                    }
                    tokens.Add(new Token(Type.DIGIT, number.ToString()));
                    continue;
                }

                else if (c == '+')
                {
                    tokens.Add(new Token(Type.ADD));
                    pos++;
                    continue;
                }

                else if (c == '-')
                {
                    tokens.Add(new Token(Type.SUB));
                    pos++;
                    continue;
                }

                else if (c == '"')
                {
                    tokens.Add(new Token(Type.OPENCLOSESTR));
                    pos++;
                    string? word = string.Empty;
                    try
                    {
                        while (pos < source.Length && source[pos] is not '"')
                        {
                            word += source[pos];
                            pos++;
                        }
                        tokens.Add(new Token(Type.STRING, word.ToString()));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    if (pos < source.Length && source[pos] == '"')
                    {
                        tokens.Add(new Token(Type.OPENCLOSESTR));
                        pos++;
                        continue;
                    }
                    else
                    {
                        throw new Exception($"Expected {Type.OPENCLOSESTR} but received {source[pos]}");
                    }
                }

                else if (c == '\'')
                {
                    tokens.Add(new Token(Type.OPENCLOSECHAR));
                    pos++;
                    string? letter = "";
                    try
                    {
                        while (pos < source.Length && source[pos] is not '\'')
                        {
                            letter += source[pos];
                            pos++;
                        }
                        bool IsChar = char.TryParse(letter, out char result);
                        if (IsChar) tokens.Add(new Token(Type.CHAR, letter));
                        else throw new Exception("SCHAR error: expected a char ('') but received another type");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    if (pos < source.Length && source[pos] == '\'')
                    {
                        tokens.Add(new Token(Type.OPENCLOSECHAR));
                        pos++;
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
                        while (pos < source.Length && char.IsLetterOrDigit(source[pos]))
                        {
                            word += source[pos];
                            pos++;
                        }

                        if (keywords.TryGetValue(word, out Type type)) {
                            tokens.Add(new Token(type));
                        }
                        else
                        {
                            tokens.Add(new Token(Type.VAR, word.ToString()));
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
