namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Analysis;
using Data;
using Interpreter.Serialization;
using Interpreter.Bytecode;

internal sealed partial class ScriptBuilder {
    private CompilerOptions Options { get; }
    private AntlrErrorListener ErrorListener { get; }
    
    private List<Warning> Warnings { get; }
    private ImportHandler ImportHandler { get; }

    public ScriptBuilder(CompilerOptions options) {
        Options = options;
        ErrorListener = new AntlrErrorListener(this);

        Warnings = [];
        ImportHandler = new ImportHandler();
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
        byte[] data = Enumerable.Range(0, 64).Select(x => (byte)(x * 2)).ToArray();
        Instruction[] instructions = Enumerable.Repeat(new Instruction(), 4).ToArray();
        
        Script script = new(data, instructions);

        return script;
    }
}