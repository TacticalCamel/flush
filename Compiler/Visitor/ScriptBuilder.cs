namespace Compiler.Visitor;

using Analysis;

internal sealed class ScriptBuilder(string sourceCode) {
    public string SourceCode { get; } = sourceCode;
    public List<string> Instructions { get; } = new();
    public List<CompilerWarning> Warnings { get; } = new();
    
    public string? ModuleName { get; set; } = null;
    public HashSet<string> ImportedModules { get; } = new();
    public bool AutoImportEnabled { get; set; } = false;
}