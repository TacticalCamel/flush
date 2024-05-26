namespace Interpreter;

using Types;
using Structs;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using Runtime.Internal;

/// <summary>
/// This class is responsible for loading defined types.
/// </summary>
public static unsafe class ClassLoader {
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

    /// <summary>
    /// All the types available in the runtime.
    /// </summary>
    private static Type[] RuntimeTypes { get; } = typeof(Runtime.Core.FlushObject).Assembly.GetExportedTypes();

    /// <summary>
    /// Load the initially included types from the runtime.
    /// </summary>
    /// <param name="imports">The initially imported modules.</param>
    /// <param name="types">The initially imported types.</param>
    public static void LoadRuntimeInitial(out HashSet<string> imports, out List<TypeDefinition> types) {
        imports = [DEFAULT_INCLUDE];
        types = [];

        LoadRuntime(false, imports, types);
    }

    /// <summary>
    /// Loads the specified modules from the runtime
    /// </summary>
    /// <param name="auto">Whether auto import is enabled.</param>
    /// <param name="imports">The imported modules.</param>
    /// <param name="types">The list that types will be added to.</param>
    public static void LoadRuntime(bool auto, HashSet<string> imports, List<TypeDefinition> types) {
        foreach (Type type in RuntimeTypes) {
            // get type module
            string? module = GetTypeModule(type);

            // invalid module
            if (module is null) {
                continue;
            }

            // type excluded by force
            if (module == DEFAULT_EXCLUDE) {
                continue;
            }

            // type was not imported and should not be visible, because
            // 1. auto import is disabled
            // 2. is not explicitly imported
            if (!auto && !imports.Contains(module)) {
                continue;
            }

            // get type name
            string name = GetTypeName(type);

            // already loaded
            if (types.Any(x => x.Name == name)) {
                continue;
            }

            // load type
            TypeDefinition typeDefinition = new() {
                Id = GenerateTypeId(module, name),
                Modifiers = default,
                IsReference = type.IsClass,
                Name = name,
                Fields = [],
                Methods = [],
                Size = GetTypeSize(type),
                GenericParameterCount = type.GenericTypeArguments.Length
            };

            // add type to the list
            types.Add(typeDefinition);
        }
    }

    /// <summary>
    /// Get the name of a native type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The name of the type.</returns>
    public static string GetTypeName(Type type) {
        return type.GetCustomAttribute<AliasAttribute>()?.Name ?? type.Name;
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

        // direct size of the type
        return (ushort)Marshal.SizeOf(type);
    }
    
    /// <summary>
    /// Get the unique identifier for a type.
    /// </summary>
    /// <param name="module">The module of the type.</param>
    /// <param name="name">The name of the type.</param>
    /// <returns>An identifier that is unique and consistent for the given type module and name</returns>
    public static Guid GenerateTypeId(ReadOnlySpan<char> module, ReadOnlySpan<char> name) {
        // get input length
        int inputLength = module.Length + name.Length;
        
        // create a span for the input
        Span<byte> source = inputLength > 1024 ? new byte[inputLength] : stackalloc byte[inputLength];
        
        // copy input to the span
        Encoding.ASCII.GetBytes(module, source);
        Encoding.ASCII.GetBytes(name, source[module.Length..]);
        
        // create a span for the hash
        Span<byte> destination = stackalloc byte[MD5.HashSizeInBytes];

        // hash the input to the destination span
        MD5.HashData(source, destination);
        
        // create a new guid from the hash
        return new Guid(destination);
    }
}