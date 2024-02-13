namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Analysis;
using Interpreter.Serialization;
using Interpreter.Bytecode;

internal sealed partial class ScriptBuilder {
    private CompilerOptions Options { get; }
    private HashSet<string> ModuleImports { get; }
    private string? Module { get; set; }
    private bool AutoImportEnabled { get; set; }

    public ScriptBuilder(CompilerOptions options) {
        Options = options;
        ErrorListener = new AntlrErrorListener(this);
        Warnings = [];
        ModuleImports = [];
        Module = null;
        AutoImportEnabled = false;
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
    
    private void SetModule(string module) {
        Module = module;
    }

    private bool AddImport(string module) {
        return ModuleImports.Add(module);
    }

    private bool EnableAutoImports() {
        bool alreadyEnabled = AutoImportEnabled;
        AutoImportEnabled = true;
        return !alreadyEnabled;
    }
}