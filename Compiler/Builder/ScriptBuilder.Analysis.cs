namespace Compiler.Builder;

using Antlr4.Runtime;
using Analysis;

internal partial class ScriptBuilder {
    public void BindToLexerErrorListener(Lexer lexer) {
        lexer.AddErrorListener(IssueHandler);
    }

    public void BindToParserErrorListener(Parser parser) {
        parser.AddErrorListener(IssueHandler);
    }

    public string[] GetIssuesWithSeverity(Severity severity) {
        Severity minimumSeverity = severity switch {
            Severity.Warning when Options.TreatWarningsAsErrors => Severity.Error,
            Severity.Error when Options.TreatWarningsAsErrors => Severity.Warning,
            _ => severity
        };

        return IssueHandler
            .Where(issue => issue.Severity >= minimumSeverity && issue.Severity <= severity && (issue.Severity >= Severity.Error || !Options.IgnoredIssues.Contains(issue.Id)))
            .OrderBy(issue => issue.Position)
            .Select(issue => issue.ToString(severity))
            .ToArray();
    }

    private void CancelIfHasErrors() {
        Severity errorSeverity = Options.TreatWarningsAsErrors ? Severity.Warning : Severity.Error;

        if (IssueHandler.Any(issue => issue.Severity >= errorSeverity && (issue.Severity >= Severity.Error || !Options.IgnoredIssues.Contains(issue.Id)))) {
            throw new OperationCanceledException();
        }
    }
}