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
                if (source[^1] != '#') throw new Exception("End of line (#) required.");
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

                else if (char.IsDigit(c))
                {
                    string number = string.Empty;

                    while(pos < source.Length && char.IsDigit(source[pos]))
                    {
                        number += source[pos];
                        pos++;
                    }
                    tokens.Add(new Token(Type.DIGIT, number.ToString()));
                    continue;
                }

                else if(c == '+')
                {
                    tokens.Add(new Token(Type.ADD));
                    pos++;
                    continue;
                }

                else if(c == '-')
                {
                    tokens.Add(new Token(Type.SUB));
                    pos++;
                    continue;
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
