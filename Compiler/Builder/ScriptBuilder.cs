namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Analysis;
using Data;
using Grammar;
using Interpreter.Serialization;
using Interpreter.Bytecode;
using Interpreter.Types;
using Microsoft.Extensions.Logging;

internal sealed partial class ScriptBuilder(CompilerOptions options, ILogger logger) : ScrantonBaseVisitor<object?> {
    private CompilerOptions Options { get; } = options;
    private ILogger Logger { get; } = logger;
    
    private WarningHandler WarningHandler { get; } = [];
    private InstructionHandler Instructions { get; } = [];
    
    private ImportHandler ImportHandler { get; } = new();
    private DataHandler DataHandler { get; } = new();
    private ClassLoader ClassLoader { get; } = new();

    public void Build(ProgramContext programContext) {
        // lexer or parser error before traversing tree
        CancelIfHasErrors();
        
        // visit root rule
        VisitProgram(programContext);
    }

    public Script GetResult() {
        // error while assembling program
        CancelIfHasErrors();
        
        byte[] data = DataHandler.ToBytes();
        Instruction[] instructions = Instructions.ToArray();
        
        return new Script(data, instructions);
    }
}