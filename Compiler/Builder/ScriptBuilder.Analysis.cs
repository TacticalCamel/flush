namespace Compiler.Builder;

using Antlr4.Runtime;
using Data;

internal partial class ScriptBuilder {
    public void BindToLexerErrorListener(Lexer lexer) {
        lexer.AddErrorListener(IssueHandler);
    }

    public void BindToParserErrorListener(Parser parser) {
        parser.AddErrorListener(IssueHandler);
    }

    public string[] GetIssuesWithLevel(Severity level) {
        Severity minLevel = level;

        if (Options.TreatWarningsAsErrors && level == Severity.Warning) minLevel = Severity.Error;
        if (Options.TreatWarningsAsErrors && level == Severity.Error) minLevel = Severity.Warning;
        
        return IssueHandler.Where(x => x.Level >= minLevel && x.Level <= level).Select(x => x.ToString(level)).ToArray();
    }

    private void CancelIfHasErrors() {
        Severity errorLevel = Options.TreatWarningsAsErrors ? Severity.Warning : Severity.Error;

        if (IssueHandler.Any(x => x.Level >= errorLevel)) {
            throw new OperationCanceledException();
        }
    }
}