namespace Compiler.Analysis;

internal sealed class WarningType(WarningLevel level, int id, string message){
    public int Id{ get; } = id;
    public WarningLevel Level{ get; } = level;
    public string Message{ get; } = message;

    public override string ToString(){
        return $"{Level} SRA{Id:D3}: {Message}";
    }

    public static WarningType AutoImportAlreadyEnabled{ get; } = new(WarningLevel.Warning, 201, "Auto import is already enabled");
    public static WarningType ModuleAlreadyImported{ get; } = new(WarningLevel.Warning, 202, "Module is already imported");
}