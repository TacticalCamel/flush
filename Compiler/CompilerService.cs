using System.Reflection;

[assembly: CLSCompliant(false)]

namespace Compiler;

using Analysis;
using Grammar;
using Visitor;
using Antlr4.Runtime;
using ProgramContext = Grammar.ScrantonParser.ProgramContext;

public static class CompilerService {
    private static Dictionary<string, Assembly> LoadedModules { get; } = new();

    public static object? Compile(string code, CompilerOptions options = CompilerOptions.Static) {
        AntlrInputStream inputStream = new(code);
        ScrantonLexer lexer = new(inputStream);
        CommonTokenStream tokenStream = new(lexer);
        ScrantonParser parser = new(tokenStream);

        ScrantonVisitor visitor = new();
        ProgramContext context = parser.program();

        object? r = visitor.VisitProgram(context);

        foreach (CompilerWarning warning in visitor.Script.Warnings.OrderBy(x => x.Start)) {
            ConsoleColor color = warning.Level switch {
                WarningLevel.Hint => ConsoleColor.Green,
                WarningLevel.Warning => ConsoleColor.Yellow,
                WarningLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.Gray
            };

            Console.ForegroundColor = color;
            Console.WriteLine(warning);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        Console.WriteLine();

        Console.WriteLine($"auto imports {(visitor.Script.AutoImportEnabled ? "on" : "off")}");
        foreach (string i in visitor.Script.ImportedModules) {
            Console.WriteLine($"import {i}");
            
            try {
                Assembly assembly = Assembly.Load(i);
                Type[] types = assembly.GetTypes();

                Console.WriteLine($"[{string.Join(", ", (IEnumerable<Type>)types)}]");
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        Console.WriteLine();

        foreach (string i in visitor.Script.Instructions) {
            Console.WriteLine(i);
        }

        return r;
    }

    private static Assembly? LoadModule(string moduleName) {
        LoadedModules.TryGetValue(moduleName, out Assembly? assembly);

        if (assembly is not null) return assembly;

        try {
            assembly = Assembly.Load(moduleName);
            LoadedModules[moduleName] = assembly;
        }
        catch {
            // ignored
        }

        return assembly;
    }
}