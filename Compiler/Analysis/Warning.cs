namespace Compiler.Analysis;

using Antlr4.Runtime;

internal sealed class Warning {
    public required uint Id { get; init; }
    public required WarningLevel Level { get; init; }
    public required string Message { get; init; }
    public Position Position { get; }

    #region Constructors and methods

    private Warning(ParserRuleContext context) {
        Position = new Position(context.start.Line, context.start.Column);
    }

    private Warning(Position position) {
        Position = position;
    }
    
    public override string ToString() {
        return ToString(Level);
    }
    
    public string ToString(WarningLevel overrideLevel) {
        return $"({Position.Line},{Position.Column}): {overrideLevel} SRA{Id:D3}: {Message}";
    }

    #endregion

    #region Templates

    public static Warning LexerTokenInvalid(Position position, string message) => new(position) {
        Id = 101,
        Level = WarningLevel.Error,
        Message = $"Failed to match token in lexer: {message}"
    };

    public static Warning ParserInputMismatch(Position position, string message) => new(position) {
        Id = 102,
        Level = WarningLevel.Error,
        Message = $"Mismatched input in parser: {message}"
    };

    public static Warning FeatureNotImplemented(ParserRuleContext context, string name) => new(context) {
        Id = 201,
        Level = WarningLevel.Warning,
        Message = $"Feature {name} is not implemented"
    };

    public static Warning AutoImportAlreadyEnabled(ParserRuleContext context) => new(context) {
        Id = 202,
        Level = WarningLevel.Warning,
        Message = "Auto import is already enabled"
    };

    public static Warning ModuleAlreadyImported(ParserRuleContext context, string name) => new(context) {
        Id = 203,
        Level = WarningLevel.Warning,
        Message = $"Module {name} is already imported"
    };

    public static Warning DuplicateModifier(ParserRuleContext context, string modifier) => new(context) {
        Id = 204,
        Level = WarningLevel.Warning,
        Message = $"Duplicate modifier {modifier}"
    };

    public static Warning IncorrectNumberFormat(ParserRuleContext context) => new(context) {
        Id = 205,
        Level = WarningLevel.Error,
        Message = "Incorrect number format"
    };

    public static Warning IncorrectCharFormat(ParserRuleContext context) => new(context) {
        Id = 206,
        Level = WarningLevel.Error,
        Message = "Incorrect character format"
    };

    public static Warning UnknownVariableType(ParserRuleContext context, string name) => new(context) {
        Id = 207,
        Level = WarningLevel.Error,
        Message = $"Unknown variable type {name}"
    };
    
    #endregion
}