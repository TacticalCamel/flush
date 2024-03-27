namespace CLI.Options;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// A class that helps in transforming command line arguments to one or more option objects.
/// </summary>
internal sealed class OptionParser {
    /// <summary>
    /// The prefix that identifier short option names
    /// </summary>
    public const string PREFIX_SHORT = "-";

    /// <summary>
    /// The prefix that identifier long option names
    /// </summary>
    public const string PREFIX_LONG = "--";

    /// <summary>
    /// Logger to use for warning messages.
    /// </summary>
    private ILogger Logger { get; }

    /// <summary>
    /// The parsing dictionary to map option names to option values.
    /// </summary>
    private Dictionary<string, string[]> Options { get; }

    /// <summary>
    /// Dictionary to map types to methods that convert them from strings to their intended type.
    /// </summary>
    private Dictionary<Type, MethodInfo> ParseMethods { get; }

    /// <summary>
    /// Get all the remaining option keys.
    /// </summary>
    public IEnumerable<string> RemainingKeys => Options.Keys;

    /// <summary>
    /// Creates a new parser and initializes its properties.
    /// </summary>
    /// <param name="logger">The logger to use for warning messages.</param>
    /// <param name="args">The input argument array.</param>
    /// <param name="defaultKey">The key to use for options that have no key.</param>
    public OptionParser(ILogger logger, string[] args, string defaultKey) {
        Logger = logger;
        Options = [];
        ParseMethods = [];

        // create an options dictionary from an array
        for (int i = 0; i <= args.Length; i++) {
            // the starting index 
            int start = i;

            // the first string will be the name of the option
            // use the target key for the first option
            string key = start < 1 ? defaultKey : args[start - 1];

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
        MethodInfo[] methods = typeof(ParseMethods).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);

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
            ParseMethods.TryAdd(returnType, method);
        }
    }

    /// <summary>
    /// Creates an option object from the remaining options.
    /// </summary>
    /// <typeparam name="TOptions">The option type to use. It must have a default constructor.</typeparam>
    /// <returns>The option object.</returns>
    public TOptions ParseFor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOptions>() where TOptions : new() {
        // create the object with its default constructor
        TOptions result = new();

        // get the properties of the type
        PropertyInfo[] properties = typeof(TOptions).GetProperties();

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
            ParseMethods.TryGetValue(property.PropertyType, out MethodInfo? parseMethod);

            // no method found, ignore property
            // this should never happen
            if (parseMethod is null) {
                Logger.PropertyNotParseable(property.PropertyType, typeof(TOptions));
                continue;
            }

            // get values and remove the key from the dictionary
            // null return value is not possible, because key is always present in the dictionary
            string[] values = TryRemoveOption(key)!;

            // convert values to the desired type
            object? value = parseMethod.Invoke(null, [values]);

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

    /// <summary>
    /// Tries to remove an option and return the associated values.
    /// </summary>
    /// <param name="key">The name of the option to remove.</param>
    /// <returns>An array of values if the key was found, null otherwise.</returns>
    public string[]? TryRemoveOption(string key) {
        // try to get the requested value
        bool success = Options.TryGetValue(key, out string[]? results);

        // if successful, remove from dictionary
        if (success) {
            Options.Remove(key);
        }

        return results;
    }
}