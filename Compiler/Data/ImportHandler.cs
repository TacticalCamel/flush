namespace Compiler.Data;

internal sealed class ImportHandler {
    public string? Module { get; set; }
    public HashSet<string> Imports { get; } = [];
    public bool AutoImportEnabled { get; set; }

    public string[] GetVisibleModules() {
        IEnumerable<string> results = Imports;

        if (Module is not null && !Imports.Contains(Module)) {
            results = results.Append(Module);
        }
        
        return results.ToArray();
    }
}