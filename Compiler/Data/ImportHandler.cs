namespace Compiler.Data;

internal sealed class ImportHandler {
    public string? Module { get; set; }
    public HashSet<string> Imports { get; } = [];
    public bool AutoImportEnabled { get; set; }
}