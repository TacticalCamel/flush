namespace Interpreter.Types;

using System.Reflection;
using Runtime.Internal;
using Runtime.Core;

public class ClassLoader {
    private static readonly string[] DefaultInclude = ["Runtime.Core"];
    private static readonly string[] DefaultExclude = ["Runtime.Internal"];

    private List<TypeInfo> Types { get; } = [];

    public void LoadModules(string[] modules, bool auto) {
        foreach (Type type in typeof(ScrantonObject).Assembly.ExportedTypes) {
            if (DefaultExclude.Any(x => type.Namespace?.StartsWith(x) ?? false)) {
                continue;
            }
            
            string @namespace = type.Namespace?[(type.Namespace.IndexOf('.') + 1)..]?.ToLower() ?? string.Empty;
            
            if (DefaultInclude.Any(x => type.Namespace?.StartsWith(x) ?? false)) {
                LoadType(type, @namespace);
                continue;
            }
            
            if (!auto) {

                if (!modules.Contains(@namespace)) {
                    continue;
                }
            }
            
            LoadType(type, @namespace);
        }
    }

    public TypeInfo? GetTypeByName(string name) {
        return Types.FirstOrDefault(x => x.Name == name);
    }

    private void LoadType(Type type, string module) {
        TypeInfo typeInfo = new() {
            Module = module,
            Name = type.GetCustomAttribute<AliasAttribute>()?.Name ?? type.Name,
            Type = type
        };
        
        //Console.WriteLine($"{typeInfo.Module}.{typeInfo.Name}");
        //Console.WriteLine(string.Join('\n', type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Select(x => $"{(x.Attributes & MethodAttributes.SpecialName) != 0} {x})")));
        
        Types.Add(typeInfo);
    }
}