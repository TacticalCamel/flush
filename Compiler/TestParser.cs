namespace Compiler;

using Antlr4.Runtime;

public static class TestParser {
    public static void Parse(string code) {
        AntlrInputStream inputStream = new(code);
        
        ScrantonLexer lexer = new(inputStream);

        CommonTokenStream tokenStream = new(lexer);
        
        ScrantonParser parser = new(tokenStream);
    }
}

