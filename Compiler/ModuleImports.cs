namespace Compiler; 

internal sealed class ModuleImports {
    public string[] ModuleNames { get; }
    public bool AutoImportEnabled { get; }

    public ModuleImports(string[] moduleNames, bool autoImportEnabled) {
        ModuleNames = moduleNames;
        AutoImportEnabled = autoImportEnabled;
    }
}