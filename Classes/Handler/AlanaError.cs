using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Handler
{
    public class AlanaError : Exception
    {
        public int line, col;
        public string source;

        public AlanaError(string message, int line, int col, string source) : base(message)
        {
            this.line = line;
            this.col = col;
            this.source = source;
        }

        public override string ToString()
        {
            string pointer = new string(' ', col - 1) + "^";

            return $"Alana: at line {line}, column {col}:\n" +
                   $"\t{source}\n" +
                   $"\t{pointer}\n" +
                   $"{Message}";
        }
    }
}
