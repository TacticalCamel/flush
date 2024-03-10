namespace Compiler.Builder;

using Antlr4.Runtime;
using Analysis;

internal partial class ScriptBuilder {
    public void BindToLexerErrorListener(Lexer lexer) {
        lexer.AddErrorListener(WarningHandler);
    }

    public void BindToParserErrorListener(Parser parser) {
        parser.AddErrorListener(WarningHandler);
    }

    public string[] GetWarningsWithLevel(WarningLevel level) {
        WarningLevel minLevel = level;

        if (Options.TreatWarningsAsErrors && level == WarningLevel.Warning) minLevel = WarningLevel.Error;
        if (Options.TreatWarningsAsErrors && level == WarningLevel.Error) minLevel = WarningLevel.Warning;
        
        return WarningHandler.Where(x => x.Level >= minLevel && x.Level <= level).Select(x => x.ToString(level)).ToArray();
    }

    private void CancelIfHasErrors() {
        WarningLevel errorLevel = Options.TreatWarningsAsErrors ? WarningLevel.Warning : WarningLevel.Error;

        if (WarningHandler.Any(x => x.Level >= errorLevel)) {
            throw new OperationCanceledException();
        }
    }
}