namespace Interpreter.Types;

using System.Reflection;
using Runtime.Internal;
using Runtime.Core;

public static class ClassLoader {
    private const string RUNTIME_ROOT_NAMESPACE = "runtime.";
    private const string DEFAULT_INCLUDE = "core";
    private const string DEFAULT_EXCLUDE = "internal";
    
    public static List<TypeInfo> LoadModules(string[] modules, bool auto) {
        List<TypeInfo> types = [];
        
        // iterate through every public type in the runtime assembly
        foreach (Type type in typeof(ScrantonObject).Assembly.ExportedTypes) {
            // every type should have a namespace
            // check anyway to compiler doesn't cry
            if (type.Namespace is null) {
                continue;
            }

            // lowercase variant of original namespace
            string module = type.Namespace.ToLower();

            // trim start if in runtime, skip if not
            // every type should be in a folder inside the runtime namespace
            if (module.StartsWith(RUNTIME_ROOT_NAMESPACE)) {
                module = module[RUNTIME_ROOT_NAMESPACE.Length..];
            }
            else {
                continue;
            }

            // type excluded by force
            if (DEFAULT_EXCLUDE.Any(excluded => module.StartsWith(excluded))) {
                continue;
            }

            // type was not imported and should not be visible, because
            // 1. auto import is disabled
            // 2. is not explicitly imported
            // 3. is not visible by default
            if (!auto && !modules.Contains(module) && !DEFAULT_INCLUDE.Any(included => module.StartsWith(included))) {
                continue;
            }

            // import type
            string name = GetTypeName(type);

            TypeInfo typeInfo = new() {
                Module = module,
                Name = name,
                Members = []
            };

            types.Add(typeInfo);
        }

        return types;
    }

    public static string GetTypeName(Type type) {
        return type.GetCustomAttribute<AliasAttribute>()?.Name ?? type.Name;
    }
}