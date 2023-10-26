namespace Compiler.Analysis;

using Antlr4.Runtime;

internal sealed class CompilerWarning(WarningLevel level, int id, string message, ParserRuleContext rule) {
    public WarningLevel Level { get; } = level;
    public int Id { get; } = id;
    public string Message { get; } = message;
    public Position Start { get; } = new(rule.start.Line, rule.start.Column);
    public Position Stop { get; } = new(rule.stop.Line, rule.stop.Column);

    public override string ToString() {
        return $"{Start} {Level} SRA{Id:D3}: {Message}";
    }
}
