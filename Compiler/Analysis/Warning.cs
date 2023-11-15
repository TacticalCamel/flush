namespace Compiler.Analysis;

using Antlr4.Runtime;

internal sealed class Warning(WarningType type, ParserRuleContext rule){
    public WarningType Type{ get; } = type;
    public Position Start { get; } = new(rule.start.Line, rule.start.Column);
    public Position Stop { get; } = new(rule.stop.Line, rule.stop.Column);

    public override string ToString() {
        return $"{Start}-{Stop} {Type}";
    }
}
