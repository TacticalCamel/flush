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
            Instruction instruction = Script.Instructions.Span[InstructionPointer];
            
            switch (instruction.OperationCode) {
                case OperationCode.Exit:
                    break;
                case OperationCode.Return:
                    break;
                case OperationCode.Call:
                    break;
                case OperationCode.Push:
                    break;
                case OperationCode.Pop:
                    break;
                case OperationCode.Jump:
                    break;
                case OperationCode.ConditionalJump:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            InstructionPointer++;
        }
    }
    
}