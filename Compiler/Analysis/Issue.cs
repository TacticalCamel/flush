namespace Compiler.Analysis;

using Antlr4.Runtime;
using Data;

/// <summary>
/// Represents a problem in the source code.
/// </summary>
internal sealed class Issue {
    /// <summary>
    /// The unique identifier of the issue.
    /// </summary>
    public required uint Id { get; init; }

    ///
    /// <summary> The severity of the issue.
    /// </summary>
    public required Severity Severity { get; init; }

    /// <summary>
    /// The message of the issue.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// The location of the issue in the source file.
    /// </summary>
    public FilePosition Position { get; }

    /// <summary>
    /// Create a new issue instance. Not intended for outside use,
    /// create issues from templates instead.
    /// </summary>
    /// <param name="context">The syntax tree node where the issue occured.</param>
    private Issue(ParserRuleContext context) {
        Position = new FilePosition(context.start.Line, context.start.Column);
    }

    /// <summary>
    /// Create a new issue instance. Not intended for outside use,
    /// create issues from templates instead.
    /// </summary>
    /// <param name="position">The position of the issue in the source file.</param>
    private Issue(FilePosition position) {
        Position = position;
    }

    /// <summary>
    /// Return a string that represents the current issue.
    /// </summary>
    /// <param name="overrideLevel">The severity to use.</param>
    /// <returns>A string that represents the current object.</returns>
    public string ToString(Severity overrideLevel) {
        return $"{Position}: {overrideLevel} SRA{Id:D3}: {Message}";
    }

    /// <summary>
    /// Return a string that represents the current issue.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() {
        return ToString(Severity);
    }

    #region Templates

    // these templates provide the only way to create issue objects

    public static Issue LexerTokenInvalid(FilePosition position, string message) => new(position) {
        Id = 101,
        Severity = Severity.Error,
        Message = $"Failed to match token in lexer: {message}"
    };

    public static Issue ParserInputMismatch(FilePosition position, string message) => new(position) {
        Id = 102,
        Severity = Severity.Error,
        Message = $"Mismatched input in parser: {message}"
    };

    public static Issue FeatureNotImplemented(ParserRuleContext context, string name) => new(context) {
        Id = 201,
        Severity = Severity.Error,
        Message = $"Feature {name} is not implemented"
    };

    public static Issue AutoImportAlreadyEnabled(ParserRuleContext context) => new(context) {
        Id = 202,
        Severity = Severity.Warning,
        Message = "Auto import is already enabled"
    };

    public static Issue ModuleAlreadyImported(ParserRuleContext context, string name) => new(context) {
        Id = 203,
        Severity = Severity.Warning,
        Message = $"Module {name} is already imported"
    };

    public static Issue IntegerTooLarge(ParserRuleContext context) => new(context) {
        Id = 204,
        Severity = Severity.Error,
        Message = "Integer value is too large"
    };

    public static Issue InvalidFloatFormat(ParserRuleContext context) => new(context) {
        Id = 205,
        Severity = Severity.Error,
        Message = "Floating-point value is in an invalid format"
    };

    public static Issue UnknownEscapeSequence(ParserRuleContext context, char character) => new(context) {
        Id = 206,
        Severity = Severity.Error,
        Message = $"Unknown escape sequence \\{character}"
    };

    public static Issue UnclosedEscapeSequence(ParserRuleContext context) => new(context) {
        Id = 207,
        Severity = Severity.Error,
        Message = "Unclosed escape sequence"
    };

    public static Issue UnrecognizedType(ParserRuleContext context, string name) => new(context) {
        Id = 208,
        Severity = Severity.Error,
        Message = $"Unknown type {name}"
    };

    public static Issue InvalidUnicodeEscape(ParserRuleContext context, int length) => new(context) {
        Id = 209,
        Severity = Severity.Error,
        Message = $"Unicode character escapes must be {length}-digit long"
    };

    public static Issue InvalidCharFormat(ParserRuleContext context) => new(context) {
        Id = 210,
        Severity = Severity.Error,
        Message = "Invalid char format"
    };

    public static Issue InvalidBinaryOperation(ParserRuleContext context, TypeIdentifier left, TypeIdentifier right, string op) => new(context) {
        Id = 211,
        Severity = Severity.Error,
        Message = $"Operator {op} cannot be applied to '{left}' and '{right}'"
    };
    
    public static Issue InvalidUnaryOperation(ParserRuleContext context, TypeIdentifier type, string op) => new(context) {
        Id = 212,
        Severity = Severity.Error,
        Message = $"Operator {op} cannot be applied to '{type}'"
    };

    public static Issue InvalidCast(ParserRuleContext context, TypeIdentifier source, TypeIdentifier target) => new(context) {
        Id = 213,
        Severity = Severity.Error,
        Message = $"Type '{source}' cannot be cast to '{target}'"
    };
    
    public static Issue ExplicitCastRedundant(ParserRuleContext context, TypeIdentifier source, TypeIdentifier target) => new(context) {
        Id = 214,
        Severity = Severity.Warning,
        Message = $"Explicit cast from '{source}' to '{target}' is redundant"
    };
    
    public static Issue ProgramEmpty(ParserRuleContext context) => new(context) {
        Id = 215,
        Severity = Severity.Warning,
        Message = "The compiled program contains no instructions"
    };
    
    public static Issue InvalidModifier(ParserRuleContext context, string name) => new(context) {
        Id = 216,
        Severity = Severity.Error,
        Message = $"'{name}' is not a valid modifier"
    };
    
    public static Issue DuplicateModifier(ParserRuleContext context, string name) => new(context) {
        Id = 217,
        Severity = Severity.Error,
        Message = $"Modifier '{name}' is already present"
    };
    
    public static Issue UnknownVariable(ParserRuleContext context, string name) => new(context) {
        Id = 218,
        Severity = Severity.Error,
        Message = $"Variable '{name}' does not exist in the current context"
    };
    
    public static Issue GenericParameterCountMismatch(ParserRuleContext context, string typeName, int expected, int actual) => new(context) {
        Id = 219,
        Severity = Severity.Error,
        Message = $"{typeName} expects {expected} generic parameters, but got {actual}"
    };

    #endregion
}