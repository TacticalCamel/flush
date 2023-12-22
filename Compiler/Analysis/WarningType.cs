namespace Compiler.Analysis;

internal sealed class WarningType(WarningLevel level, uint id, string message){
    public uint Id{ get; } = id;
    public WarningLevel Level{ get; } = level;
    public string Message{ get; } = message;
    
    public static WarningType FeatureNotImplemented{ get; } = new(WarningLevel.Error, 100, "Feature is not implemented");
    public static WarningType AutoImportAlreadyEnabled{ get; } = new(WarningLevel.Warning, 201, "Auto import is already enabled");
    public static WarningType ModuleAlreadyImported{ get; } = new(WarningLevel.Warning, 202, "Module is already imported");
}