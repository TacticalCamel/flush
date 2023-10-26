using System.Linq.Expressions;
using System.Reflection;

[assembly: CLSCompliant(false)]

namespace Compiler;

using Analysis;
using Grammar;
using Visitor;
using Antlr4.Runtime;
using ProgramContext = Grammar.ScrantonParser.ProgramContext;

public static class CompilerService{
    private static Dictionary<string, Assembly> LoadedModules{ get; } = new();

    public static object? Compile(string code, CompilerOptions options = CompilerOptions.Static){
        AntlrInputStream inputStream = new(code);
        ScrantonLexer lexer = new(inputStream);
        CommonTokenStream tokenStream = new(lexer);
        ScrantonParser parser = new(tokenStream);

        ProgramContext context = parser.program();
        ScriptBuilder scriptBuilder = new(code);
        ScrantonVisitor visitor = new(scriptBuilder);

        Console.ForegroundColor = ConsoleColor.DarkBlue;
        try{
            visitor.TraverseAst(context);
            Console.WriteLine("Successful compilation");
        }
        catch (OperationCanceledException){
            Console.WriteLine("Compilation cancelled after error");
        }
        catch (Exception e){
            Console.WriteLine($"Unexpected compilation error: {e.Message}");
        }
        Console.WriteLine();

        DebugInfo(scriptBuilder);
        return null;
    }

    private static Assembly? LoadModule(string moduleName){
        LoadedModules.TryGetValue(moduleName, out Assembly? assembly);

        if (assembly is not null) return assembly;

        try{
            assembly = Assembly.Load(moduleName);
            LoadedModules[moduleName] = assembly;
        }
        catch{
            // ignored
        }

        return assembly;
    }

    private static void DebugInfo(ScriptBuilder scriptBuilder){
        LogLn("WARNINGS [");
        foreach (CompilerWarning warning in scriptBuilder.Warnings.OrderBy(x => x.Start)){
            ConsoleColor color = warning.Level switch{
                WarningLevel.Hint => ConsoleColor.Green,
                WarningLevel.Warning => ConsoleColor.Yellow,
                WarningLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.Gray
            };

            LogLn($"    {warning}", color);
        }
        LogLn("]\n");
        
        LogLn($"MODULE [\n    {scriptBuilder.ModuleName}\n]\n");

        LogLn($"IMPORTS (auto={(scriptBuilder.AutoImportEnabled ? "true" : "false")}) [");
        foreach (string module in scriptBuilder.ImportedModules){
            Log($"\"{module}\": ");
            
            Assembly? assembly = LoadModule(module);

            if (assembly is null){
                LogLn("<not-found>", ConsoleColor.Red);
            }
            else{
                Type[] types = assembly.GetTypes();
                LogLn($"[{string.Join(", ", (IEnumerable<Type>)types)}]", ConsoleColor.Green);
            }
        }
        LogLn("]\n");
        

        LogLn("INSTRUCTIONS [");
        foreach (string instruction in scriptBuilder.Instructions){
            ConsoleColor color = instruction.StartsWith("SKIP") ? ConsoleColor.Red : ConsoleColor.Green;
            LogLn($"    {instruction}", color);
        }
        LogLn("]");
        
        return;

        void Log(object? obj, ConsoleColor color = ConsoleColor.Gray){
            Console.ForegroundColor = color;
            Console.Write(obj);
        }

        void LogLn(object? obj = null, ConsoleColor color = ConsoleColor.Gray){
            Log(obj, color);
            Console.WriteLine();
        }
    }
}