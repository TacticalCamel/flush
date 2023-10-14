
[assembly: CLSCompliant(false)]

namespace Compiler;

using Analysis;
using Grammar;
using Visitor;
using Antlr4.Runtime;
using ProgramContext = Grammar.ScrantonParser.ProgramContext;

public static class CompilerService {
    public static object? Compile(string code, CompilerOptions options = CompilerOptions.Static) {
        AntlrInputStream inputStream = new(code);
        ScrantonLexer lexer = new(inputStream);
        CommonTokenStream tokenStream = new(lexer);
        ScrantonParser parser = new(tokenStream);
        
        ScrantonVisitor visitor = new();
        ProgramContext context = parser.program();
        
        object? r = visitor.VisitProgram(context);

        foreach (var message in visitor.Messages.OrderBy(x => x.Start)) {
            ConsoleColor color = message.Level switch {
                WarningLevel.Hint => ConsoleColor.Green,
                WarningLevel.Warning => ConsoleColor.Yellow,
                WarningLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.Gray
            };

            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        
        Console.WriteLine();

        foreach (var i in visitor.Instructions) {
            Console.WriteLine(i);
        }
        
        return r;
    }

}

