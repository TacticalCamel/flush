namespace Compiler.Visitor;

using Analysis;

/*
1. Resolve imports
2. Resolve known types and functions
3. Transform code
*/

internal sealed class Script {
    public string? ModuleName { get; set; } = null;
    public HashSet<string> ImportedModules { get; } = new();
    public bool AutoImportEnabled { get; set; } = false;
    public List<CompilerWarning> Warnings { get; } = new();
    public List<string> Instructions { get; } = new();
    
    
}