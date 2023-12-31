[assembly: CLSCompliant(false)]

namespace Compiler;

using Grammar;
using Visitor;
using Antlr4.Runtime;
using Microsoft.Extensions.Logging;

public sealed class CompilerService(ILogger logger) {
    private ILogger Logger { get; } = logger;

    public void Build(string code, CompilerOptions options) {
        ScriptBuilder scriptBuilder = new();

        AntlrInputStream inputStream = new(code);
        ScrantonLexer lexer = new(inputStream);
        lexer.AddErrorListener(scriptBuilder);

        CommonTokenStream tokenStream = new(lexer);
        ScrantonParser parser = new(tokenStream);
        parser.AddErrorListener(scriptBuilder);

        ScrantonVisitor visitor = new(parser.program(), scriptBuilder);
        
        try {
            visitor.TraverseAst();
            Logger.BuildSuccessful();
        }
        catch (OperationCanceledException) {
            Logger.BuildFailed();
        }
        catch (Exception e) {
            Logger.UnexpectedBuildError(e);
        }
        
        Logger.Debug(scriptBuilder);
    }
}