namespace Interpreter;

using Types;
using System.Reflection;
using Runtime.Internal;
using Runtime.Core;

public static class ClassLoader {
    /// <summary>
    /// The namespace which is included by force.
    /// Contains core types that should always be available.
    /// </summary>
    private const string DEFAULT_INCLUDE = "core";
    
    /// <summary>
    /// The namespace which is excluded by force.
    /// Contains internal types that should not be used.
    /// </summary>
    private const string DEFAULT_EXCLUDE = "internal";

    // TODO not here?
    public static Version BytecodeVersion => typeof(ClassLoader).Assembly.GetName().Version ?? new Version();
    
    public static List<TypeDefinition> LoadNativeModules(string[] modules, bool auto) {
        List<TypeDefinition> types = [];

        // iterate through every public type in the runtime assembly
        foreach (Type type in typeof(ScrantonObject).Assembly.ExportedTypes) {
            // get type module
            string? module = GetTypeModule(type);

            // invalid module
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
                Name = name,
                Modifiers = Modifier.None,
                Fields = [],
                Methods = [],
                StackSize = GetTypeSize(type),
                IsReference = type.IsClass
            };
            
            types.Add(typeDefinition);
        }

        return types;
    }

    /// <summary>
    /// Get the module of a type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The module of the type if successful, null otherwise.</returns>
    private static string? GetTypeModule(Type type) {
        const string RUNTIME_ROOT_NAMESPACE = "Runtime.";

        // invalid namespace
        // every type should be in a folder inside the runtime namespace
        if (type.Namespace is null || !type.Namespace.StartsWith(RUNTIME_ROOT_NAMESPACE)) {
            return null;
        }

        // convert to lower case variant
        return type.Namespace[RUNTIME_ROOT_NAMESPACE.Length..].ToLower();
    }

    /// <summary>
    /// Get the size of a native type in bytes.
    /// Must not be a generic struct.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The size of the type if successful, 0 otherwise.</returns>
    private static ushort GetTypeSize(Type type) {
        // reference size
        if (type.IsClass) {
            return sizeof(ulong);
        }

        // generic struct
        if (type.IsGenericType) {
            return 0;
        }
        
        // direct size of the type
        return (ushort)Marshal.SizeOf(type);
    }

    /// <summary>
    /// Get the name of a native type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The name of the type.</returns>
    public static string GetTypeName(Type type) {
        return type.GetCustomAttribute<AliasAttribute>()?.Name ?? type.Name;
    }
}