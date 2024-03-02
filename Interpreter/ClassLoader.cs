
namespace Interpreter;

using System.Reflection;
using Runtime.Internal;
using Runtime.Core;

public static class ClassLoader {
    public static Dictionary<string, Type> Types { get; }
    
    static ClassLoader() {
        Types = typeof(ScrantonObject).Assembly.ExportedTypes.Where(x => !x.Namespace?.StartsWith("Runtime.Internal") ?? false).ToDictionary(x => x.GetCustomAttribute<AliasAttribute>()?.Name ?? x.Name, x => x);

        /*foreach (KeyValuePair<string, Type> pair in Types) {
            Console.WriteLine($"{pair.Key} -> {pair.Value}");
            MethodInfo[] methods = pair.Value.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            foreach (MethodInfo method in methods) {
                Console.WriteLine($"\t{method}");
            }
        }*/
    }
}