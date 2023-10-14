namespace Compiler.Visitor;

/*
1. Resolve imports
2. Resolve known types and functions
3. Transform code
*/

internal sealed class Script(string sourceCode) {
    public string SourceCode { get; } = sourceCode;
}