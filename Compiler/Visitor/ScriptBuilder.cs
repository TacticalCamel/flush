namespace Compiler.Visitor;

using System.Text;
using Antlr4.Runtime;
using Analysis;

internal sealed class ScriptBuilder {
    public ScriptBuilder(CompilerOptions options) {
        Options = options;
        ErrorListener = new AntlrErrorListener(this);
        Warnings = [];
        ModuleImports = [];
        Module = null;
        AutoImportEnabled = false;
    }

    private CompilerOptions Options { get; }
    private AntlrErrorListener ErrorListener { get; }
    private List<Warning> Warnings { get; }
    private HashSet<string> ModuleImports { get; }
    private string? Module { get; set; }
    private bool AutoImportEnabled { get; set; }

    public void SetModule(string module) {
        Module = module;
    }

    public bool AddImport(string module) {
        return ModuleImports.Add(module);
    }

    public bool EnableAutoImports() {
        bool alreadyEnabled = AutoImportEnabled;
        AutoImportEnabled = true;
        return !alreadyEnabled;
    }

    public void BindToLexerErrorListener(Lexer lexer) {
        lexer.AddErrorListener(ErrorListener);
    }

    public void BindToParserErrorListener(Parser parser) {
        parser.AddErrorListener(ErrorListener);
    }

    public void AddWarning(WarningType type, ParserRuleContext rule, string? message = null) {
        AddWarning(type, new Position(rule.start.Line, rule.start.Column), message);
    }

    public void AddWarning(WarningType type, Position position, string? message = null) {
        if (type.Level <= WarningLevel.Warning && Options.IgnoredWarningIds.Contains(type.Id)) {
            return;
        }
        
        Warning warning = new(type, position, message);
        Warnings.Add(warning);
    }

    public bool HasErrors() {
        WarningLevel errorLevel = Options.TreatWarningsAsErrors ? WarningLevel.Warning : WarningLevel.Error;
        return Warnings.Any(x => x.Type.Level >= errorLevel);
    }

    public string[] GetWarningsWithLevel(WarningLevel level) {
        WarningLevel minLevel = level;

        if (Options.TreatWarningsAsErrors && level == WarningLevel.Warning) minLevel = WarningLevel.Error;
        if (Options.TreatWarningsAsErrors && level == WarningLevel.Error) minLevel = WarningLevel.Warning;
        
        return Warnings.Where(x => x.Type.Level >= minLevel && x.Type.Level <= level).Select(x => x.ToString(level)).ToArray();
    }
    
    public override string ToString() {
        StringBuilder sb = new();

        sb.AppendLine("Warnings: {");
        foreach (Warning warning in Warnings) sb.AppendLine($"    {warning}");
        sb.AppendLine("}");

        sb.AppendLine("Module: {");
        sb.AppendLine($"    {Module}");
        sb.AppendLine("}");

        sb.AppendLine($"Imports: (auto = {(AutoImportEnabled ? "true" : "false")}) {{");
        foreach (string module in ModuleImports) sb.AppendLine($"    {module}");
        sb.AppendLine("}");

        return sb.ToString();
    }
}