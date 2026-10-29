using Classes;
using Classes.AST;
using Classes.AST.Expressions;
using Classes.Handler;
using Classes.Lexer;
using Classes.Parser;
using Classes.Runtime;
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
                    Console.WriteLine("       alana start <path>");
                    Console.WriteLine("       alana --version");
                    return;
                }

                if (args[0] == "--version")
                {
                    Console.WriteLine("Alana 2.0");
                    return;
                }

                if (args[0] == "start")
                {
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: alana start <path>");
                        return;
                    }

                    StartProject(args[1]);
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

                Interpreter interpreter = new Interpreter(file);
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

        private static void StartProject(string path)
        {
            string target;

            if (path.EndsWith(".alana", StringComparison.OrdinalIgnoreCase))
            {
                target = Path.GetFullPath(path);
                string? directory = Path.GetDirectoryName(target);

                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            else
            {
                string directory = Path.GetFullPath(path);
                Directory.CreateDirectory(directory);
                target = Path.Combine(directory, "main.alana");
            }

            if (File.Exists(target))
            {
                Console.WriteLine($"Alana error: file '{target}' already exists.");
                return;
            }

            File.WriteAllText(target, "including \"stdlib/error.alana\";\n\nshowln \"Hello, Alana\";\n");
            Console.WriteLine($"Created {target}");
        }
    }
}
