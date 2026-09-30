using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
