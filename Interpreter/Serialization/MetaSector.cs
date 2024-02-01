namespace Interpreter.Serialization;

public sealed class MetaSector {
    public required Version TargetVersion { get; init; }
    public required DateTime CompilationTime { get; init; }
}