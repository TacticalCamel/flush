[assembly: CLSCompliant(false)]

namespace Compiler;

using System.Reflection;
using Grammar;
using Visitor;
using Antlr4.Runtime;
using ProgramContext = Grammar.ScrantonParser.ProgramContext;

public static class CompilerService{
    private static Dictionary<string, Assembly> LoadedModules{ get; } = new();

    public static void Compile(string code, CompilerOptions options = CompilerOptions.Static){
        AntlrInputStream inputStream = new(code);
        ScrantonLexer lexer = new(inputStream);
        CommonTokenStream tokenStream = new(lexer);
        ScrantonParser parser = new(tokenStream);

        ProgramContext programContext = parser.program();
        ScrantonVisitor visitor = new(programContext);

        try{
            ScriptBuilder scriptBuilder = visitor.TraverseAst();
            LogLn("Successful compilation", ConsoleColor.DarkBlue);
            LogLn(scriptBuilder);
        }
        catch (OperationCanceledException){
            LogLn("Compilation cancelled after error", ConsoleColor.DarkRed);
        }
        catch (Exception e){
            LogLn($"Unexpected compilation error: {e.Message}", ConsoleColor.DarkRed);
        }

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
    
    private static void Log(object? obj, ConsoleColor color = ConsoleColor.Gray){
        Console.ForegroundColor = color;
        Console.Write(obj);
    }

    private static void LogLn(object? obj = null, ConsoleColor color = ConsoleColor.Gray){
        Log(obj, color);
        Console.WriteLine();
    }
}