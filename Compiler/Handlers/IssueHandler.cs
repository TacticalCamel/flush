namespace Compiler.Handlers;

using System.Collections;
using Antlr4.Runtime;
using Data;

/// <summary>
/// This class manages the issues thrown during compilation.
/// Issues can be added manually, and it can listen for lexer and parser errors
/// </summary>
internal sealed class IssueHandler: IAntlrErrorListener<IToken>, IAntlrErrorListener<int>, IEnumerable<Issue> {
    /// <summary> The collection of issues </summary>
    private List<Issue> Issues { get; } = [];
    
    /// <summary>
    /// Add an issue to the collection
    /// </summary>
    /// <param name="issue">The issue to add</param>
    public void Add(Issue issue) {
        Issues.Add(issue);
    }
    
    public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
        Issue issue = Issue.ParserInputMismatch(new FilePosition(line, charPositionInLine), msg);
        Issues.Add(issue);
    }

    public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
        Issue issue = Issue.LexerTokenInvalid(new FilePosition(line, charPositionInLine), msg);
        
        Issues.Add(issue);
    }

    public IEnumerator<Issue> GetEnumerator() {
        return Issues.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}