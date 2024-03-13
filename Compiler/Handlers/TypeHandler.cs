namespace Compiler.Handlers;

using Data;
using Interpreter.Types;

/// <summary>
/// This class managed the modules names imported in the program header.
/// To avoid naming conflicts, not all modules must be visible by default.
/// </summary>
internal sealed class TypeHandler {
    /// <summary>
    /// The module of the current program.
    /// Code within the same module is visible by default.
    /// Can be null, in which case the code can not be imported to other programs
    /// </summary>
    private string? ProgramModule { get; set; }
    
    /// <summary>
    /// Whether all runtime code should be visible by default.
    /// Naming conflicts might occur
    /// </summary>
    private bool AutoImportEnabled { get; set; }
    
    /// <summary>
    /// The imported modules of the program.
    /// Use a hashset to avoid duplicates
    /// </summary>
    private HashSet<string> Imports { get; } = [];

    /// <summary>
    /// The list of currently visible types
    /// </summary>
    private List<TypeInfo> Types { get; set; } = [];

    /// <summary>
    /// Gets the modules that should be visible from the current program
    /// </summary>
    /// <returns></returns>
    private string[] GetVisibleModules() {
        // imports are visible
        IEnumerable<string> results = Imports;

        // the current module is also visible
        if (ProgramModule is not null && !Imports.Contains(ProgramModule)) {
            results = results.Append(ProgramModule);
        }
        
        return results.ToArray();
    }

    /// <summary>
    /// Load all visible types
    /// </summary>
    public void LoadTypes() {
        string[] visibleModules = GetVisibleModules();

        Types = ClassLoader.LoadModules(visibleModules, AutoImportEnabled);
    }

    /// <summary>
    /// Add a new import
    /// </summary>
    /// <param name="name">The module to import</param>
    /// <returns>Whether the imported module was already present</returns>
    public bool Add(string name) {
        return Imports.Add(name);
    }

    /// <summary>
    /// Enable auto import
    /// </summary>
    /// <returns>Whether auto import was already enabled</returns>
    public bool EnableAutoImport() {
        bool alreadyEnabled = AutoImportEnabled;

        AutoImportEnabled = true;
        
        return !alreadyEnabled;
    }

    /// <summary>
    /// Set the module of the program
    /// </summary>
    /// <param name="name">The module to use</param>
    public void SetModule(string name) {
        ProgramModule = name;
    }

    /// <summary>
    /// Retrieve a type by name. Throw an exception if not found
    /// </summary>
    /// <param name="name">The name of the type</param>
    /// <returns>The type</returns>
    public TypeInfo GetByName(string name) {
        return Types.First(type => type.Name == name);
    }
    
    /// <summary>
    /// Retrieve a type by name
    /// </summary>
    /// <param name="name">The name of the type</param>
    /// <returns>The type if it was found, null otherwise</returns>
    public TypeInfo? TryGetByName(string name) {
        return Types.FirstOrDefault(type => type.Name == name);
    }

    /// <summary>
    /// Retrieve a non-generic type directly
    /// </summary>
    /// <typeparam name="T">The the type to retrieve</typeparam>
    /// <returns>The type</returns>
    public TypeIdentifier GetFromType<T>() {
        Type type = typeof(T);
        
        if (type.IsGenericType) {
            throw new ArgumentException("Type cannot be generic");
        }
        
        string name = ClassLoader.GetTypeName(type);

        TypeInfo typeInfo = GetByName(name);

        return new TypeIdentifier(typeInfo, Array.Empty<TypeIdentifier>());
    }
}