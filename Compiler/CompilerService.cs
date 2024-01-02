
[assembly: CLSCompliant(false)]

namespace Compiler;

using Grammar;
using Visitor;
using Analysis;
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
            LogBuildResults(scriptBuilder, options, true);
        }
        catch (OperationCanceledException) {
            LogBuildResults(scriptBuilder, options, false);
        }
        catch (Exception e) {
            Logger.UnexpectedBuildError(e);
        }
        
        Logger.Debug(scriptBuilder);
    }

    private void LogBuildResults(ScriptBuilder scriptBuilder, CompilerOptions options, bool success) {
        int warningCount = 0, errorCount = 0;

        foreach (Warning warning in scriptBuilder.GetWarnings()) {
            switch (warning.Type.Level) {
                case WarningLevel.Hint:
                    Logger.BuildHint(warning);
                    break;
                case WarningLevel.Warning:
                    if (options.IgnoredWarningIds.Contains(warning.Type.Id)) continue;
                    Logger.BuildWarning(warning);
                    warningCount++;
                    break;
                case >= WarningLevel.Error:
                    Logger.BuildError(warning);
                    errorCount++;
                    break;
            }
        }

        if (success) {
            Logger.BuildResultSuccess(errorCount, warningCount);
        }
        else {
            Logger.BuildResultFail(errorCount, warningCount);
        }
    }
}