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

    public string[] GetIssuesWithSeverity(Severity severity) {
        Severity minimumSeverity = severity;

        if (Options.TreatWarningsAsErrors && severity == Severity.Warning) minimumSeverity = Severity.Error;
        if (Options.TreatWarningsAsErrors && severity == Severity.Error) minimumSeverity = Severity.Warning;
        
        return IssueHandler
            .Where(issue => issue.Severity >= minimumSeverity && issue.Severity <= severity)
            .OrderBy(issue => issue.Position)
            .Select(x => x.ToString(severity))
            .ToArray();
    }

    private void CancelIfHasErrors() {
        Severity errorLevel = Options.TreatWarningsAsErrors ? Severity.Warning : Severity.Error;

        if (IssueHandler.Any(x => x.Severity >= errorLevel)) {
            throw new OperationCanceledException();
        }
    }
}