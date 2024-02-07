[assembly: CLSCompliant(false)]

namespace Compiler;

using Grammar;
using Visitor;
using Analysis;
using Antlr4.Runtime;
using Interpreter.Serialization;
using Interpreter.Bytecode;
using Microsoft.Extensions.Logging;

public sealed class CompilerService(ILogger logger, CompilerOptions options) {
    private ILogger Logger { get; } = logger;
    private CompilerOptions Options { get; } = options;

    public Script? Compile(string code) {
        ScriptBuilder scriptBuilder = new(Options);
        
        AntlrInputStream inputStream = new(code);
        ScrantonLexer lexer = new(inputStream);
        scriptBuilder.BindToLexerErrorListener(lexer);

        CommonTokenStream tokenStream = new(lexer);
        ScrantonParser parser = new(tokenStream);
        scriptBuilder.BindToParserErrorListener(parser);

        try {
            ScrantonVisitor visitor = new(parser.program(), scriptBuilder);
            visitor.TraverseAst();

            LogBuildResults(scriptBuilder, true);

            // TODO temporary test data
            byte[] data = Enumerable.Range(0, 64).Select(x => (byte)(x * 2)).ToArray();
            Instruction[] instructions = Enumerable.Repeat(new Instruction(), 4).ToArray();
            
            return new Script(data, instructions);
        }
        catch (OperationCanceledException) {
            LogBuildResults(scriptBuilder, false);
            return null;
        }
        catch (Exception e) {
            Logger.UnexpectedBuildError(e);
            return null;
        }
    }

    private void LogBuildResults(ScriptBuilder scriptBuilder, bool success) {
        string[] hints = scriptBuilder.GetWarningsWithLevel(WarningLevel.Hint);
        if(hints.Length > 0) Logger.BuildHint(string.Join(Environment.NewLine, hints));
        
        string[] warnings = scriptBuilder.GetWarningsWithLevel(WarningLevel.Warning);
        if(warnings.Length > 0) Logger.BuildWarning(string.Join(Environment.NewLine, warnings));
        
        string[] errors = scriptBuilder.GetWarningsWithLevel(WarningLevel.Error);
        if(errors.Length > 0) Logger.BuildError(string.Join(Environment.NewLine, errors));
        
        if (success) {
            Logger.BuildResultSuccess(errors.Length, warnings.Length);
        }
        else {
            Logger.BuildResultFail(errors.Length, warnings.Length);
        }
    }
}