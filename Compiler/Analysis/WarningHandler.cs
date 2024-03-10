namespace Compiler.Analysis;

using System.Collections;
using Antlr4.Runtime;

internal sealed class WarningHandler: IAntlrErrorListener<IToken>, IAntlrErrorListener<int>, IEnumerable<Warning> {
    private List<Warning> Warnings { get; } = [];
    
    public void Add(Warning warning) {
        Warnings.Add(warning);
    }
    
    public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
        Warning warning = Warning.ParserInputMismatch(new Position(line, charPositionInLine), msg);
        
        Warnings.Add(warning);
    }

    public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
        Warning warning = Warning.LexerTokenInvalid(new Position(line, charPositionInLine), msg);
        
        Warnings.Add(warning);
    }

    public IEnumerator<Warning> GetEnumerator() {
        return Warnings.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}