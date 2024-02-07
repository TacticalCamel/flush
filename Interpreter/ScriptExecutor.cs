namespace Interpreter;

using Serialization;

public sealed class ScriptExecutor(Script script) {
    public static Version BytecodeVersion => typeof(ScriptExecutor).Assembly.GetName().Version ?? new Version();

    private Script Script { get; } = script;
    
    public void Run() {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(Script);
        Console.ForegroundColor = ConsoleColor.Gray;
    }
}