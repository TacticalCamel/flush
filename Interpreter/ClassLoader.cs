namespace Interpreter;

using Types;
using System.Reflection;
using Runtime.Internal;
using Runtime.Core;

public static class ClassLoader {
    private const string DEFAULT_INCLUDE = "core";
    private const string DEFAULT_EXCLUDE = "internal";

    public static Version BytecodeVersion => typeof(ClassLoader).Assembly.GetName().Version ?? new Version();

    public static List<TypeDefinition> LoadNativeModules(string[] modules, bool auto) {
        List<TypeDefinition> types = [];

        // iterate through every public type in the runtime assembly
        foreach (Type type in typeof(ScrantonObject).Assembly.ExportedTypes) {
            // get type module
            string? module = GetTypeModule(type);

            // invalid module
            // every type should be in a folder inside the runtime namespace
            if (module is null) {
                continue;
            }

            // type excluded by force
            if (module.StartsWith(DEFAULT_EXCLUDE)) {
                continue;
            }

            // type was not imported and should not be visible, because
            // 1. auto import is disabled
            // 2. is not explicitly imported
            // 3. is not visible by default
            if (!auto && !modules.Contains(module) && !DEFAULT_INCLUDE.Any(included => module.StartsWith(included))) {
                continue;
            }

            // get type name
            string name = GetTypeName(type);
            
            // define type
            TypeDefinition typeDefinition = new() {
                Module = module,
                Name = name,
                Fields = [],
                Methods = [],
                Size = GetSize(type),
                IsReference = type.IsClass
            };
            
            types.Add(typeDefinition);
        }

        return types;
    }

    private static string? GetTypeModule(Type type) {
        const string RUNTIME_ROOT_NAMESPACE = "Runtime.";

        string? name = type.Namespace;

        // invalid namespace
        if (name is null || !name.StartsWith(RUNTIME_ROOT_NAMESPACE)) {
            return null;
        }

        return name[RUNTIME_ROOT_NAMESPACE.Length..].ToLower();
    }

    private static ushort GetSize(Type type) {
        if (type.IsClass) {
            return 8;
        }

        // stupid thinks a char is 1 byte
        if (type == typeof(Char)) {
            return 2;
        }
        
        // but booleans are 4 bytes because why not
        if (type == typeof(Bool)) {
            return 1;
        }

        return (ushort)Marshal.SizeOf(type);
    }

    public static string GetTypeName(Type type) {
        return type.GetCustomAttribute<AliasAttribute>()?.Name ?? type.Name;
    }
}