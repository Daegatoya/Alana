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
    COMA,
    OPEN,
    CLOSE,
    EXITFUNC,
    DEFFUNC,
    RETURN,
    OPENCLOSESTR,
    STRING,
    CALL,
    BOOL,
    DECIMAL,
    CHAR,
    DECIMAL_NUM,
    BOOL_C,
    OPENCLOSECHAR,
    IF,
    EQUAL_EQUAL,
    NOT,
    GREATERTHAN,
    LESSTHAN,
    AND,
    OR,
    LESSOREQUAL,
    GREATEROREQUAL,
    ELSEIF,
    ELSE,
    REDEFINE,
    NEW,
    OPENARRAY,
    CLOSEARRAY,
    PUSH,
    WITH,
    POP,
    LENGTH,
    SHOWLN,
    READLN,
    READK,
    PARSENUM,
    PARSESTR,
    PARSEBOOL,
    PARSEDEC,
    PARSECHAR,
    WHILE,
    FOR,
    FOREVERY,
    CONTINUE,
    BREAK,
}

namespace Classes.Lexer
{
    public class Token
    {
        public Type type { get; private set; }
        public string? value = string.Empty, source = string.Empty;
        public int line, col;

        public Token(Type type, int line, int col, string? source, string? value = null)
        {
            this.type = type;
            this.value = value;
            this.line = line;
            this.col = col;
            this.source = source;
        }
    }
}
