namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Analysis;
using Data;
using Grammar;
using Interpreter.Serialization;
using Interpreter.Bytecode;

internal sealed partial class ScriptBuilder: ScrantonBaseVisitor<object?> {
    private CompilerOptions Options { get; }
    private AntlrErrorListener ErrorListener { get; }
    
    private List<Warning> Warnings { get; }
    private ImportHandler ImportHandler { get; }
    private DataHandler DataHandler { get; }

    public ScriptBuilder(CompilerOptions options) {
        Options = options;
        ErrorListener = new AntlrErrorListener(this);

        Warnings = [];
        ImportHandler = new ImportHandler();
        DataHandler = new DataHandler();
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
        
        // TODO temporary test data
        byte[] data = DataHandler.ToBytes();
        Instruction[] instructions = Enumerable.Repeat(new Instruction(), 4).ToArray();
        
        Script script = new(data, instructions);

        return script;
    }
}