using System.Runtime.CompilerServices;

namespace Interpreter.Types;

using System.Reflection;
using Runtime.Internal;
using Runtime.Core;

public static class ClassLoader {
    private const string DEFAULT_INCLUDE = "core";
    private const string DEFAULT_EXCLUDE = "internal";
    
    public static Version BytecodeVersion => typeof(ClassLoader).Assembly.GetName().Version ?? new Version();
    
    public static List<TypeInfo> LoadModules(string[] modules, bool auto) {
        List<TypeInfo> types = [];
        
        // iterate through every public type in the runtime assembly
        foreach (Type type in typeof(ScrantonObject).Assembly.ExportedTypes) {
            // get module of the type
            string? module = GetTypeModule(type);

            // invalid module
            // every type should be in a folder inside the runtime namespace
            if (module is null) {
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
                Members = [],
                Size = (byte)(type.IsClass ? 8 : Marshal.SizeOf(type))
            };
            
            System.Reflection.MemberInfo[] members = type
                .GetMembers(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.Name == "op_Implicit" && x.GetCustomAttribute<InternalAttribute>() is null)
                .ToArray();

            foreach (System.Reflection.MemberInfo m in members) {
                Console.WriteLine(m.ToString() ?? "null");
            }
            

            types.Add(typeInfo);
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
    
    public static string GetTypeName(Type type) {
        return type.GetCustomAttribute<AliasAttribute>()?.Name ?? type.Name;
    }
}