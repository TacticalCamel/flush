namespace Interpreter;

using Bytecode;

public static class ScriptExecutor {
    public static Version BytecodeVersion => typeof(ScriptExecutor).Assembly.GetName().Version ?? new Version();

    public static void Run(Script script) {
        
    }
}