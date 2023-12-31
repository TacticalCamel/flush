namespace Compiler;

using System.Reflection;

public sealed class CompilerOptions {
    private const string PREFIX_SHORT = "-";
    private const string PREFIX_LONG = "--";
    
    [CompilerFlag(Name = "static", ShortName = "s", Description = "Include referenced code into the compiled program.")]
    public bool IsStatic { get; init; }

    [CompilerFlag(Name = "warnings-as-errors", ShortName = "wae", Description = "Treat warnings as if they were errors.")]
    public bool TreatWarningsAsErrors { get; init; }

    [CompilerFlag(Name = "no-meta", ShortName = "nm", Description = "Do not include metadata in the compiled program.")]
    public bool ExcludeMetaData { get; init; }

    [CompilerFlag(Name = "plain-text", ShortName = "pt", Description = "Compile to a plain text representation instead of bytecode. The output file is not executable, but human-readable for debug purposes.")]
    public bool CompileToPlainText { get; init; }
    
    [CompilerFlag(Name = "execute-only", ShortName = "exe", Description = "Do not create an output file, only compile and execute in memory instead.")]
    public bool ExecuteOnly { get; init; }
    
    public static CompilerOptions FromConsoleArgs(string[] args) {
        CompilerOptions options = new();

        PropertyInfo[] properties = typeof(CompilerOptions).GetProperties();
        Dictionary<string, PropertyInfo> flags = [];

        foreach (PropertyInfo property in properties) {
            CompilerFlagAttribute? attribute = property.GetCustomAttribute<CompilerFlagAttribute>();
            if (attribute is null) continue;

            flags.TryAdd($"{PREFIX_LONG}{attribute.Name}", property);
            flags.TryAdd($"{PREFIX_SHORT}{attribute.ShortName}", property);
        }

        foreach (string arg in args) {
            flags.TryGetValue(arg, out PropertyInfo? property);
            property?.SetValue(options, true);
        }

        return options;
    }

    [AttributeUsage(AttributeTargets.Property)]
    private sealed class CompilerFlagAttribute: Attribute {
        public required string Name { get; init; }
        public required string ShortName { get; init; }
        public required string Description { get; init; }
    }
}
