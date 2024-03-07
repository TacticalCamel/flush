namespace Compiler.Analysis;

using Antlr4.Runtime;

internal sealed class AntlrErrorListener(List<Warning> warnings): IAntlrErrorListener<IToken>, IAntlrErrorListener<int> {
    private List<Warning> Warnings { get; } = warnings;
    
    public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
        Warning warning = Warning.ParserInputMismatch(new Position(line, charPositionInLine), msg);
        
        Warnings.Add(warning);
    }

    public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
        Warning warning = Warning.LexerTokenInvalid(new Position(line, charPositionInLine), msg);
        
        Warnings.Add(warning);
    }
}
