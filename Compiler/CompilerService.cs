namespace Compiler;

using Grammar;
using Antlr4.Runtime;
using ProgramContext = Grammar.ScrantonParser.ProgramContext;

public static class CompilerService {
    public static object? Compile(string code) {
        ProgramContext context = GetContext(code);

        ScrantonVisitor visitor = new();
        var r = visitor.VisitProgram(context);
        
        /*
        string imports = string.Join(", ", context.program_header().import_segment().import_statement().Select(x => x.children[1].GetText()));
        Console.WriteLine($"imports = [{imports}]");
        */
        
        return null;
    }

    private static ProgramContext GetContext(string code) {
        AntlrInputStream inputStream = new(code);
        ScrantonLexer lexer = new(inputStream);
        CommonTokenStream tokenStream = new(lexer);
        ScrantonParser parser = new(tokenStream);
        
        return parser.program();
    }
}

