namespace Compiler.Analysis;

internal sealed class Warning(WarningType type, Position start, Position finish, string? message = null) {
	public WarningType Type { get; } = type;
	public Position Start { get; } = start;
	public Position Finish { get; } = finish;
	public string? Message { get; } = message;

	public override string ToString() {
		return $"{Type.Level} SRA{Type.Id:D3} at ({Start.Line}, {Start.Column}): {Type.Message}{(Message is null ? string.Empty : $" ({Message})")}";
	}
}
