using Classes;
using Classes.AST;
using Classes.AST.Expressions;
using Classes.Handler;
using Classes.Interpreter;
using Classes.Lexer;
using Classes.Parser;
using System;

namespace Alana
{
    public static class Alana
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: alana <file.alana>");
                    return;
                }
                string file = args[0];
                if (!File.Exists(file))
                {
                    Console.WriteLine($"Alana error: file '{file}' not found.");
                    return;
                }

                string source = File.ReadAllText(file);

                Lexer lexer = new Lexer(source);
                Token[] tokens = lexer.TranslateToken();

                Parser parser = new Parser(tokens);
                List<Statement> statements = parser.Parse();

                Interpreter interpreter = new Interpreter();
                interpreter.Execute(statements);
            }
            catch (AlanaError ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Alana error: {ex.Message}");
            }
        }
    }
}