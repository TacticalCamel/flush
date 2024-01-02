namespace Compiler.Analysis;

internal sealed class WarningType(WarningLevel level, uint id, string message){
    public uint Id{ get; } = id;
    public WarningLevel Level{ get; } = level;
    public string Message{ get; } = message;

    #region Compiler options

    public static WarningType UnknownCompilerFlag{ get; } = new(WarningLevel.Warning, 1, "");
    public static WarningType NoCompilerInput{ get; } = new(WarningLevel.Warning, 2, "");
    public static WarningType CompilerInputNotFound{ get; } = new(WarningLevel.Warning, 3, "");

    #endregion
    
    #region Parser and lexer

    public static WarningType LexerTokenInvalid{ get; } = new(WarningLevel.Error, 101, "Failed to match token in lexer");
    public static WarningType ParserInputMismatch{ get; } = new(WarningLevel.Error, 102, "Mismatched input in parser");

    #endregion

    #region Assembler

    public static WarningType FeatureNotImplemented{ get; } = new(WarningLevel.Warning, 201, "Feature is not implemented");
    public static WarningType AutoImportAlreadyEnabled{ get; } = new(WarningLevel.Warning, 202, "Auto import is already enabled");
    public static WarningType ModuleAlreadyImported{ get; } = new(WarningLevel.Warning, 203, "Module is already imported");
    public static WarningType DuplicateModifier{ get; } = new(WarningLevel.Warning, 204, "Duplicate modifier");
    
    #endregion
}
