namespace Compiler; 

internal sealed class ModuleImports {
    public string[] ModuleNames { get; }
    public bool AutoImportsEnabled { get; }

    public ModuleImports(string[] moduleNames, bool autoImportsEnabled) {
        ModuleNames = moduleNames;
        AutoImportsEnabled = autoImportsEnabled;
    }
}