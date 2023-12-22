namespace Compiler.Analysis;

using Antlr4.Runtime;

internal sealed class Warning(WarningType type, ParserRuleContext rule){
    private WarningType Type{ get; } = type;
    public Position Start { get; } = new(rule.start.Line, rule.start.Column);
    public Position Finish { get; } = new(rule.stop.Line, rule.stop.Column);

    public override string ToString() {
        return $"{Type.Level} SRA{Type.Id:D3} at ({Start.Line}, {Start.Column}): {Type.Message}";
    }
}
