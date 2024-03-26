namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Handlers;
using Grammar;
using Interpreter.Serialization;
using Interpreter.Bytecode;
using Microsoft.Extensions.Logging;

internal sealed partial class ScriptBuilder(CompilerOptions options, ILogger logger) : ScrantonBaseVisitor<object?> {
    private CompilerOptions Options { get; } = options;
    private ILogger Logger { get; } = logger;
    
    private IssueHandler IssueHandler { get; } = [];
    private InstructionHandler Instructions { get; } = [];
    private TypeHandler TypeHandler { get; } = new();
    private DataHandler DataHandler { get; } = new();

    public void Build(ProgramContext programContext) {
        // lexer or parser error before traversing tree
        CancelIfHasErrors();

        // create a preprocessor
        Preprocessor preprocessor = new(Options, IssueHandler, TypeHandler);

        // traverse AST: 1st pass
        preprocessor.VisitProgram(programContext);
        
        // check for errors in 1st pass
        CancelIfHasErrors();
        
        // traverse AST: 2nd pass
        VisitProgram(programContext);
        
        // check for errors in 2nd pass
        CancelIfHasErrors();
    }

    public Script GetResult() {
        byte[] data = DataHandler.ToBytes();
        Instruction[] instructions = Instructions.ToArray();
        
        return new Script(data, instructions);
    }

    public override object? VisitProgram(ProgramContext context) {
        return VisitChildren(context);
    }

    public override object? VisitProgramBody(ProgramBodyContext context) {
        return VisitChildren(context);
    }
}