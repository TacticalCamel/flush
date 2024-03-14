namespace Interpreter;

using Serialization;
using Bytecode;

public sealed class ScriptExecutor(Script script, ILogger logger) {
    public static Version BytecodeVersion => typeof(ScriptExecutor).Assembly.GetName().Version ?? new Version();

    private Script Script { get; } = script;
    private ILogger Logger { get; } = logger;
    private int InstructionPointer { get; set; }
    private List<object?> Stack { get; } = [];
    
    public void Run() {
        Logger.ExecutingScript(Script);

        while (InstructionPointer < Script.Instructions.Length) {
            Instruction i = Script.Instructions.Span[InstructionPointer];
            
            /*switch (i.Code) {
                default:
                    throw new ArgumentOutOfRangeException();
            }*/

            InstructionPointer++;
        }
    }
    
}