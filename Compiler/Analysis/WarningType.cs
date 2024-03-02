namespace Compiler.Analysis;

internal sealed class WarningType {
    public uint Id { get; }
    public WarningLevel Level { get; }
    public string Message { get; }

    private WarningType(uint id, WarningLevel level, string message) {
        Id = id;
        Level = level;
        Message = message;
    }

    #region Parser and lexer

    public static WarningType LexerTokenInvalid { get; } = new(101, WarningLevel.Error, "Failed to match token in lexer");
    public static WarningType ParserInputMismatch { get; } = new(102, WarningLevel.Error, "Mismatched input in parser");

    #endregion

    
    #region Assembler

    public static WarningType FeatureNotImplemented { get; } = new(201, WarningLevel.Warning, "Feature is not implemented");
    public static WarningType AutoImportAlreadyEnabled { get; } = new(202, WarningLevel.Warning, "Auto import is already enabled");
    public static WarningType ModuleAlreadyImported { get; } = new(203, WarningLevel.Warning, "Module is already imported");
    public static WarningType DuplicateModifier { get; } = new(204, WarningLevel.Warning, "Duplicate modifier");
    public static WarningType IncorrectNumberFormat { get; } = new(205, WarningLevel.Error, "Incorrect number format");
    public static WarningType IncorrectCharFormat { get; } = new(206, WarningLevel.Error, "Incorrect character format");

    #endregion
}