namespace ConsoleInterface.Options;

using System.Diagnostics.CodeAnalysis;

internal sealed class OptionsParser {
    private const string PREFIX_SHORT = "-";
    private const string PREFIX_LONG = "--";

    private ILogger Logger { get; }
    private Dictionary<string, string[]> Options { get; }
    private Dictionary<Type, MethodInfo> ParseFunctions { get; }

    public OptionsParser(ILogger logger, string[] args, string targetKey) {
        Logger = logger;
        Options = [];
        ParseFunctions = [];

        for (int i = 0; i <= args.Length; i++) {
            int start = i;
            string key = start < 1 ? targetKey : args[start - 1];

            while (i < args.Length && !args[i].StartsWith(PREFIX_SHORT)) i++;

            bool success = Options.TryAdd(key, args[start..i]);

            if (!success) {
                logger.DuplicateFlag(key);
            }
        }

        MethodInfo[] methods = typeof(ParseFunctions).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);

        foreach (MethodInfo method in methods) {
            Type? returnType = method.ReturnType.IsValueType ? Nullable.GetUnderlyingType(method.ReturnType) : method.ReturnType;
            if (returnType is null) continue;

            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 1) continue;
            if (parameters[0].ParameterType != typeof(string[])) continue;

            ParseFunctions.TryAdd(returnType, method);
        }
    }

    public T ParseFor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>() where T : new() {
        T result = new();

        PropertyInfo[] properties = typeof(T).GetProperties();
        Dictionary<string, PropertyInfo> flags = [];

        foreach (PropertyInfo property in properties) {
            DisplayAttribute? attribute = property.GetCustomAttribute<DisplayAttribute>();
            if (attribute is null) continue;

            if (attribute.Name is not null) {
                flags.TryAdd($"{PREFIX_LONG}{attribute.Name}", property);
            }

            if (attribute.ShortName is not null) {
                flags.TryAdd($"{PREFIX_SHORT}{attribute.ShortName}", property);
            }
        }

        foreach (string key in Options.Keys) {
            flags.TryGetValue(key, out PropertyInfo? property);
            if (property is null) continue;

            ParseFunctions.TryGetValue(property.PropertyType, out MethodInfo? parseFunction);

            if (parseFunction is null) {
                Logger.PropertyNotParseable(property.PropertyType, typeof(T));
                continue;
            }

            object?[] parameters = [Options[key]];
            object? value = parseFunction.Invoke(null, parameters);

            if (value is null) {
                Logger.InvalidFlagValue(key, Options[key]);
            }
            else {
                property.SetValue(result, value);
            }

            Options.Remove(key);
        }

        return result;
    }

    public IEnumerable<string> GetRemainingOptionNames() {
        return Options.Keys;
    }

    public string[]? GetAndRemoveOption(string key) {
        bool success = Options.TryGetValue(key, out string[]? value);

        if (success) {
            Options.Remove(key);
        }

        return value;
    }
}