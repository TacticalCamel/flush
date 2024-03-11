namespace Compiler.Data;

/// <summary>
/// This class managed the modules names imported in the program header.
/// To avoid naming conflicts, not all namespaces must be visible by default.
/// </summary>
internal sealed class ImportHandler {
    /// <summary>
    /// The module of the current program.
    /// Code within the same module is visible by default.
    /// Can be null, in which case the code can not be imported to other programs
    /// </summary>
    public string? Module { get; set; }
    
    /// <summary>
    /// The imported modules of the program.
    /// Use a hashset to avoid duplicates
    /// </summary>
    public HashSet<string> Imports { get; } = [];
    
    /// <summary>
    /// Enable automatic imports.
    /// This will make all outside code visible.
    /// Naming conflicts might occur
    /// </summary>
    public bool AutoImportEnabled { get; set; }

    /// <summary>
    /// Gets the modules that should be visible from the current program
    /// </summary>
    /// <returns></returns>
    public string[] GetVisibleModules() {
        // imports are visible
        IEnumerable<string> results = Imports;

        // the current module is also visible
        if (Module is not null && !Imports.Contains(Module)) {
            results = results.Append(Module);
        }
        
        return results.ToArray();
    }
}