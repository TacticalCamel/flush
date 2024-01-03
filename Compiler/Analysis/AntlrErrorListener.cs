namespace Compiler.Analysis;

using Antlr4.Runtime;
using Visitor;

internal sealed class AntlrErrorListener(ScriptBuilder scriptBuilder): IAntlrErrorListener<IToken>, IAntlrErrorListener<int> {
    private ScriptBuilder ScriptBuilder { get; } = scriptBuilder;
    
    public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
        ScriptBuilder.AddWarning(WarningType.ParserInputMismatch, new Position(line, charPositionInLine), msg);
    }

    public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
        ScriptBuilder.AddWarning(WarningType.LexerTokenInvalid, new Position(line, charPositionInLine), msg);
    }
}
