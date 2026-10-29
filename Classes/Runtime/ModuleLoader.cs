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
            LoadFile(fullPath, "Imported");
        }

        public void Include(string path)
        {
            string root = InstallRoot();
            string fullPath = Path.GetFullPath(Path.Combine(root, path));
            string rootPrefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase) && !string.Equals(fullPath, Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase))
            {
                throw runtime.Error($"Included file is outside the Alana folder: {path}");
            }

            LoadFile(fullPath, "Included");
        }

        public static string InstallRoot()
        {
            string? env = Environment.GetEnvironmentVariable("ALANA_HOME");

            if (!string.IsNullOrWhiteSpace(env) && Directory.Exists(env))
            {
                return Path.GetFullPath(env);
            }

            string baseDir = AppContext.BaseDirectory;
            string parent = Path.GetFullPath(Path.Combine(baseDir, ".."));

            if (Directory.Exists(Path.Combine(parent, "stdlib")))
            {
                return parent;
            }

            if (Directory.Exists(Path.Combine(baseDir, "stdlib")))
            {
                return baseDir;
            }

            return parent;
        }

        private void LoadFile(string fullPath, string kind)
        {
            if (!File.Exists(fullPath))
            {
                throw runtime.Error($"{kind} file not found: {fullPath}");
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