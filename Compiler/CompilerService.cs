[assembly: CLSCompliant(false)]

namespace Compiler;

using Grammar;
using Builder;
using Data;
using Antlr4.Runtime;
using Interpreter.Serialization;
using Microsoft.Extensions.Logging;

public sealed class CompilerService(ILogger logger, CompilerOptions options) {
    private ILogger Logger { get; } = logger;
    private CompilerOptions Options { get; } = options;

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
            scriptBuilder.Build(parser.program());

            // retrieve result
            Script script = scriptBuilder.GetResult();
            
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

    private void LogBuildResults(ScriptBuilder scriptBuilder, bool success) {
        // get and log hints
        string[] hints = scriptBuilder.GetIssuesWithSeverity(Severity.Hint);
        if(hints.Length > 0) Logger.BuildHint(string.Join(Environment.NewLine, hints));
        
        // get and log warnings
        string[] warnings = scriptBuilder.GetIssuesWithSeverity(Severity.Warning);
        if(warnings.Length > 0) Logger.BuildWarning(string.Join(Environment.NewLine, warnings));
        
        // get and log errors
        string[] errors = scriptBuilder.GetIssuesWithSeverity(Severity.Error);
        if(errors.Length > 0) Logger.BuildError(string.Join(Environment.NewLine, errors));
        
        // log summary
        if (success) {
            Logger.BuildResultSuccess(errors.Length, warnings.Length);
        }
        else {
            Logger.BuildResultFail(errors.Length, warnings.Length);
        }
    }
}