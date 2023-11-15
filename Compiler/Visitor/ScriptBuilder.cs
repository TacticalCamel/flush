namespace Compiler.Visitor;

using System.Text;
using Antlr4.Runtime;
using Analysis;

internal sealed class ScriptBuilder{
    private List<Warning> Warnings { get; } = new();
    private HashSet<string> ModuleImports { get; } = new();
    private ScopeTracker ScopeTracker{ get; } = new();
    
    private string? Module { get; set; }
    private bool AutoImportEnabled{ get; set; }

    public void AddWarning(WarningType type, ParserRuleContext rule){
        Warnings.Add(new Warning(type, rule));
    }

    public void SetModule(string module){
        Module = module;
    }

    public bool AddImport(string module){
        return ModuleImports.Add(module);
    }

    public bool EnableAutoImports(){
        bool alreadyEnabled = AutoImportEnabled;
        AutoImportEnabled = true;
        return !alreadyEnabled;
    }

    public override string ToString(){
        StringBuilder sb = new();
        
        sb.AppendLine("Warnings: {");
        foreach (Warning warning in Warnings.OrderBy(x => x.Start)) sb.AppendLine($"    {warning}");
        sb.AppendLine("}");
        
        sb.AppendLine("Module: {");
        sb.AppendLine($"    {Module}");
        sb.AppendLine("}");
        
        sb.AppendLine($"Imports: (auto = {(AutoImportEnabled ? "true" : "false")}) {{");
        foreach (string module in ModuleImports) sb.AppendLine($"    {module}");
        sb.AppendLine("}");
        
        /*sb.AppendLine("Instructions: {");
        foreach (string instruction in Instructions) sb.AppendLine($"    {instruction}");
        sb.AppendLine("}");*/

        return sb.ToString();
    }
}