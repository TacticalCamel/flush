namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Analysis;
using Data;
using Grammar;
using Interpreter.Serialization;
using Interpreter.Bytecode;
using Interpreter.Types;

internal sealed partial class ScriptBuilder: ScrantonBaseVisitor<object?> {
    private CompilerOptions Options { get; }
    private List<Warning> Warnings { get; }
    private AntlrErrorListener ErrorListener { get; }
    private ImportHandler ImportHandler { get; }
    private DataHandler DataHandler { get; }
    private List<Instruction> Instructions { get; }
    private ClassLoader ClassLoader { get; }

    public ScriptBuilder(CompilerOptions options) {
        Options = options;
        Warnings = [];
        ErrorListener = new AntlrErrorListener(Warnings);
        ImportHandler = new ImportHandler();
        DataHandler = new DataHandler();
        Instructions = [];
        ClassLoader = new ClassLoader();
    }

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