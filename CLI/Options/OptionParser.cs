namespace CLI.Options;

using System.Diagnostics.CodeAnalysis;

internal sealed class OptionParser {
    public const string PREFIX_SHORT = "-";
    public const string PREFIX_LONG = "--";

    private ILogger Logger { get; }
    private Dictionary<string, string[]> Options { get; }
    private Dictionary<Type, MethodInfo> ParseFunctions { get; }

    public OptionParser(ILogger logger, string[] args, string targetKey) {
        Logger = logger;
        Options = [];
        ParseFunctions = [];

        // create an options dictionary from an array
        for (int i = 0; i <= args.Length; i++) {
            // the starting index 
            int start = i;
            
            // the first string will be the name of the option
            // use the target key for the first option
            string key = start < 1 ? targetKey : args[start - 1];

            // traverse array until the end or the next key is reached
            while (i < args.Length && !args[i].StartsWith(PREFIX_SHORT)) i++;

            // try to add the option
            bool success = Options.TryAdd(key, args[start..i]);

            // option with the same name was already added
            // current option was no added and ignored instead
            if (!success) {
                logger.DuplicateFlag(key);
            }
        }

        // get parse methods from the dedicated class
        MethodInfo[] methods = typeof(ParseFunctions).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);

        // create a dictionary for the parse methods
        foreach (MethodInfo method in methods) {
            // get the actual return type
            Type? returnType = method.ReturnType.IsValueType ? Nullable.GetUnderlyingType(method.ReturnType) : method.ReturnType;
            
            // method must have a return value
            if (returnType is null) continue;

            // method must have exactly 1 parameter, which is a string array
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 1) continue;
            if (parameters[0].ParameterType != typeof(string[])) continue;

            // add the method
            // duplicates should not exist, but avoid exceptions anyway
            ParseFunctions.TryAdd(returnType, method);
        }
    }

    public T ParseFor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>() where T : new() {
        // create the object with its default constructor
        T result = new();
        
        // get the properties of the type
        PropertyInfo[] properties = typeof(T).GetProperties();
        
        // dictionary to map option names to the properties of the type
        Dictionary<string, PropertyInfo> flags = [];

        // for every property in the object
        foreach (PropertyInfo property in properties) {
            // get the display attribute of the property
            DisplayAttribute? attribute = property.GetCustomAttribute<DisplayAttribute>();
            
            // if not present, ignore
            if (attribute is null) continue;

            // map property to an option with a long name
            if (attribute.Name is not null) {
                flags.TryAdd(PREFIX_LONG + attribute.Name, property);
            }

            // map property to an option with a short name
            if (attribute.ShortName is not null) {
                flags.TryAdd(PREFIX_SHORT + attribute.ShortName, property);
            }
        }

        // update properties of the object depending on the available options
        foreach (string key in Options.Keys) {
            // get the property mapped to the current option name
            flags.TryGetValue(key, out PropertyInfo? property);
            
            // if not present, ignore
            // the current option name may still be valid for another type
            if (property is null) continue;

            // search for a method that parses the values to the correct property type
            ParseFunctions.TryGetValue(property.PropertyType, out MethodInfo? parseFunction);

            // no method found, ignore property
            // this should never happen
            if (parseFunction is null) {
                Logger.PropertyNotParseable(property.PropertyType, typeof(T));
                continue;
            }

            // get values and remove the key from the dictionary
            // null return value is not possible, because key is always present in the dictionary
            string[] values = GetAndRemoveOption(key)!;
            
            // convert values to the desired type
            object? value = parseFunction.Invoke(null, [values]);

            // values were not in a valid format, display warning message
            if (value is null) {
                Logger.InvalidFlagValue(key, values);
            }
            // success, set property in the object
            else {
                property.SetValue(result, value);
            }
        }

        return result;
    }

    public IEnumerable<string> RemainingKeys => Options.Keys;

    public string[]? GetAndRemoveOption(string key) {
        // try to get the requested value
        bool success = Options.TryGetValue(key, out string[]? value);

        // if successful, remove from dictionary
        if (success) {
            Options.Remove(key);
        }

        return value;
    }
}