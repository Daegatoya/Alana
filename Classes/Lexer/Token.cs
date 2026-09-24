using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum Type
{
    NUM,
    DIGIT,
    ADD,
    SUB,
    END,
    VAR,
    EQUAL,
    DEFINE,
    SHOW,
    MULTI,
    DIVIDE,
    STR,
}

namespace Classes.Lexer
{
    public class Token
    {
        public Type type { get; private set; }
        public string? value = string.Empty;

        public Token(Type type, string? value = null)
        {
            this.type = type;
            this.value = value;
        }
    }
}
