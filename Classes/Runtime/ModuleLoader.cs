using Classes.AST;
using Classes.Lexer;
using Classes.Parser;
using System;
using System.Collections.Generic;
using System.IO;

namespace Classes.Runtime
{
    public class ModuleLoader
    {
        private readonly Stack<string> filesStack = new();
        private readonly HashSet<string> loaded = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> loading = new(StringComparer.OrdinalIgnoreCase);
        private readonly Interpreter runtime;

        public string CurrentFile => filesStack.Peek();

        public ModuleLoader(Interpreter runtime, string entryFile)
        {
            this.runtime = runtime;
            string fullPath = Path.GetFullPath(entryFile);
            filesStack.Push(fullPath);
            loading.Add(fullPath);
        }

        public void Load(string path)
        {
            string currentDirectory = Path.GetDirectoryName(CurrentFile)!;
            string fullPath = Path.GetFullPath(Path.Combine(currentDirectory, path));

            if (!File.Exists(fullPath))
            {
                throw runtime.Error($"Imported file not found: {fullPath}");
            }

            if (loaded.Contains(fullPath))
            {
                return;
            }

            if (loading.Contains(fullPath))
            {
                return;
            }

            loading.Add(fullPath);

            string source = File.ReadAllText(fullPath);

            Lexer.Lexer lexer = new Lexer.Lexer(source);
            Token[] tokens = lexer.TranslateToken();

            Parser.Parser parser = new Parser.Parser(tokens);
            List<Statement> statements = parser.Parse();

            EnterModule(fullPath);

            try
            {
                runtime.Execute(statements);
                loaded.Add(fullPath);
            }
            finally
            {
                loading.Remove(fullPath);
                ExitModule();
            }
        }

        public void EnterModule(string file)
        {
            filesStack.Push(Path.GetFullPath(file));
        }

        public void ExitModule()
        {
            if (filesStack.Count <= 1) return;

            filesStack.Pop();
        }

        public bool IsCurrentModule(string file)
        {
            return Path.GetFullPath(file).Equals(Path.GetFullPath(CurrentFile), StringComparison.OrdinalIgnoreCase);
        }
    }
}