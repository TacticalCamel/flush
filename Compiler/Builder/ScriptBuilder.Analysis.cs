namespace Compiler.Builder;

using Antlr4.Runtime;
using Analysis;

internal partial class ScriptBuilder {
    /// <summary>
    /// Binds the instance to listen for lexer errors.
    /// </summary>
    /// <param name="lexer">The lexer.</param>
    public void BindToLexerErrorListener(Lexer lexer) {
        lexer.AddErrorListener(IssueHandler);
    }

    /// <summary>
    /// Binds the instance to listen for parser errors.
    /// </summary>
    /// <param name="parser">The parser.</param>
    public void BindToParserErrorListener(Parser parser) {
        parser.AddErrorListener(IssueHandler);
    }

    /// <summary>
    /// Gets all issues with the specified severity.
    /// </summary>
    /// <param name="severity">The severity to use.</param>
    /// <returns>An array of issues converted to strings.</returns>
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

    /// <summary>
    /// Cancels the compilation process if an error occured.
    /// </summary>
    /// <exception cref="OperationCanceledException">Thrown when there is at least 1 error.</exception>
    private void CancelIfHasErrors() {
        Severity errorSeverity = Options.TreatWarningsAsErrors ? Severity.Warning : Severity.Error;

        if (IssueHandler.Any(issue => issue.Severity >= errorSeverity && (issue.Severity >= Severity.Error || !Options.IgnoredIssues.Contains(issue.Id)))) {
            throw new OperationCanceledException();
        }
    }
}