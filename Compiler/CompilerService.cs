// best place to put this if we don't want to create a different file.
// necessary because the generated classes have this attribute,
// but the containing assembly doesn't so the C# compiler cries.

[assembly: CLSCompliant(false)]

namespace Compiler;

using Grammar;
using Builder;
using Analysis;
using Antlr4.Runtime;
using Interpreter.Bytecode;
using Microsoft.Extensions.Logging;

/// <summary>
/// This class is the public interface of the compiler.
/// It is wrapper responsible for directing the compilation process.
/// </summary>
/// <param name="options">The settings of the compiler.</param>
/// <param name="logger">The logger used by the compiler.</param>
public sealed class CompilerService(CompilerOptions options, ILogger logger) {
    /// <summary>
    /// The settings of the compiler.
    /// </summary>
    private CompilerOptions Options { get; } = options;

    /// <summary>
    /// The logger used by the compiler.
    /// </summary>
    private ILogger Logger { get; } = logger;

    /// <summary>
    /// Transform the given source code to an executable.
    /// </summary>
    /// <param name="code">The source code.</param>
    /// <returns>An executable script if successful, null otherwise.</returns>
    public Script? Compile(string code) {
        // create a new builder
        ScriptBuilder scriptBuilder = new(Options, Logger);

        // create lexer and listen to errors
        AntlrInputStream inputStream = new(code);
        ScrantonLexer lexer = new(inputStream);
        scriptBuilder.BindToLexerErrorListener(lexer);

        // create parser and listen to errors
        CommonTokenStream tokenStream = new(lexer);
        ScrantonParser parser = new(tokenStream);
        scriptBuilder.BindToParserErrorListener(parser);

        try {
            // build program
            Script script = scriptBuilder.Build(parser.program());

            // log results
            LogBuildResults(scriptBuilder, true);
            return script;
        }
        catch (OperationCanceledException) {
            // log results
            // this exception is thrown when 1 or more errors are encountered
            LogBuildResults(scriptBuilder, false);
            return null;
        }
        catch (Exception e) {
            // no other types of exceptions should be thrown
            // this is a failsafe to catch programmer errors
            Logger.UnexpectedBuildError(e);
            return null;
        }
    }

    /// <summary>
    /// Log the results of the compilation.
    /// </summary>
    /// <param name="scriptBuilder">The script builder used for the compilation.</param>
    /// <param name="success">True if the compilation was successful, false otherwise.</param>
    private void LogBuildResults(ScriptBuilder scriptBuilder, bool success) {
        // get and log hints
        string[] hints = scriptBuilder.GetIssuesWithSeverity(Severity.Hint);
        if (hints.Length > 0) Logger.BuildHint(string.Join(Environment.NewLine, hints));

        // get and log warnings
        string[] warnings = scriptBuilder.GetIssuesWithSeverity(Severity.Warning);
        if (warnings.Length > 0) Logger.BuildWarning(string.Join(Environment.NewLine, warnings));

        // get and log errors
        string[] errors = scriptBuilder.GetIssuesWithSeverity(Severity.Error);
        if (errors.Length > 0) Logger.BuildError(string.Join(Environment.NewLine, errors));

        // log summary
        if (success) {
            Logger.BuildResultSuccess(errors.Length, warnings.Length);
        }
        else {
            Logger.BuildResultFail(errors.Length, warnings.Length);
        }
    }
}