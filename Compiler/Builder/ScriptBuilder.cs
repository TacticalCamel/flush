namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Handlers;
using Grammar;
using Analysis;
using Interpreter.Bytecode;

/// <summary>
/// Implements the traversal of the syntax tree with the visitor pattern. 
/// </summary>
/// <param name="options">The setting to use during compilation.</param>
internal sealed partial class ScriptBuilder(CompilerOptions options) : ScrantonBaseVisitor<object?> {
    /// <summary>
    /// The setting to use during compilation.
    /// This is the only form of state the builder is initialized with.
    /// </summary>
    private CompilerOptions Options { get; } = options;

    /// <summary>
    /// This issue handler to manage compilation issues. 
    /// </summary>
    private IssueHandler IssueHandler { get; } = [];

    /// <summary>
    /// The data handler to manage the data section.
    /// </summary>
    private DataHandler DataHandler { get; } = new();

    /// <summary>
    /// The type handler to manage loaded types.
    /// </summary>
    private TypeHandler TypeHandler { get; } = new();

    /// <summary>
    /// The code handler to manage instructions and program state.
    /// </summary>
    private CodeHandler CodeHandler { get; } = new();

    /// <summary>
    /// Indicates that preprocessor mode is enabled.
    /// Visit methods may change behaviour depending on this value.
    /// </summary>
    private bool IsPreprocessorMode { get; set; } = false;

    /// <summary>
    /// Visit a syntax tree and transform it to an executable program.
    /// </summary>
    /// <param name="programContext">The root of the syntax tree.</param>
    /// <returns>The compiled program.</returns>
    public Script Build(ProgramContext programContext) {
        // lexer or parser error
        CancelIfHasErrors();

        // traverse syntax tree
        VisitProgram(programContext);

        // check for errors
        CancelIfHasErrors();

        // warn if the code section is empty
        if (!CodeHandler.HasInstructions) {
            IssueHandler.Add(Issue.ProgramEmpty(programContext));
        }

        // get data and code sections
        byte[] data = DataHandler.GetByteArray();
        Instruction[] instructions = CodeHandler.GetInstructionArray();

        // create script
        Script script = new(data, instructions, TypeHandler.ModuleNameAddress);

        return script;
    }
}