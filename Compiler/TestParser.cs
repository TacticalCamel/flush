namespace Compiler;

using Antlr4.Runtime;

public static class TestParser {
    public static void Parse(string code) {
        AntlrInputStream inputStream = new(code);
        
        ScrantonLexer lexer = new(inputStream);

        CommonTokenStream tokenStream = new(lexer);
        
        ScrantonParser parser = new(tokenStream);

        ScrantonParser.ProgramContext? v = parser.program();

        string imports = string.Join(", ", v.import_segment().import_statement().Select(x => x.children[1].GetText()));
        
        Console.WriteLine($"imports = [{imports}]");
    }
}

