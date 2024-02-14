namespace Compiler.Builder;

using Antlr4.Runtime;
using Analysis;

internal partial class ScriptBuilder {
    public void BindToLexerErrorListener(Lexer lexer) {
        lexer.AddErrorListener(ErrorListener);
    }

    public void BindToParserErrorListener(Parser parser) {
        parser.AddErrorListener(ErrorListener);
    }

    public string[] GetWarningsWithLevel(WarningLevel level) {
        WarningLevel minLevel = level;

        if (Options.TreatWarningsAsErrors && level == WarningLevel.Warning) minLevel = WarningLevel.Error;
        if (Options.TreatWarningsAsErrors && level == WarningLevel.Error) minLevel = WarningLevel.Warning;
        
        return Warnings.Where(x => x.Type.Level >= minLevel && x.Type.Level <= level).Select(x => x.ToString(level)).ToArray();
    }
    
    public void AddWarning(WarningType type, ParserRuleContext rule, string? message = null) {
        AddWarning(type, new Position(rule.start.Line, rule.start.Column), message);
    }

    public void AddWarning(WarningType type, Position position, string? message = null) {
        if (type.Level <= WarningLevel.Warning && Options.IgnoredWarningIds.Contains(type.Id)) {
            return;
        }
        
        Warning warning = new(type, position, message);
        Warnings.Add(warning);
    }

    private void CancelIfHasErrors() {
        WarningLevel errorLevel = Options.TreatWarningsAsErrors ? WarningLevel.Warning : WarningLevel.Error;

        if (Warnings.Any(x => x.Type.Level >= errorLevel)) {
            throw new OperationCanceledException();
        }
    }
}