namespace Compiler.Analysis;

internal sealed class Warning(WarningType type, Position position, string? message = null) {
	public WarningType Type { get; } = type;
    public Position Position { get; } = position;
    public string? Message { get; } = message;

	public override string ToString() {
		return $"SRA{Type.Id:D3} at ({Position.Line}, {Position.Column}): {Type.Message}{(Message is null ? string.Empty : $" ({Message})")}";
	}
    
    public string ToString(WarningLevel overrideLevel) {
        return $"({Position.Line},{Position.Column}): {overrideLevel} SRA{Type.Id:D3}: {Type.Message}{(Message is null ? string.Empty : $" ({Message})")}";
    }
}
