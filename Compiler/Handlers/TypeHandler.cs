namespace Compiler.Handlers;

using Data;
using Interpreter.Types;

/// <summary>
/// This class manages the imported types the program sees.
/// To avoid naming conflicts, not all modules are visible by default.
/// </summary>
internal sealed class TypeHandler {
    /// <summary>
    /// Backing field for core types so the public property can have a non nullable type
    /// </summary>
    private CoreTypeHelper? CoreTypesBackingField;

    /// <summary>
    /// Helper to retrieve type identifiers for core types
    /// </summary>
    /// <exception cref="Exception">Thrown when type helper is accessed before loading types</exception>
    public CoreTypeHelper CoreTypes => CoreTypesBackingField ?? throw new Exception("Core type helper accessed before loading types");

    /// <summary>
    /// Backing field for conversion helper so the public property can have a non nullable type
    /// </summary>
    private PrimitiveConversionHelper? PrimitiveConversionsBackingField;

    /// <summary>
    /// Helper to TODO
    /// </summary>
    /// <exception cref="Exception">Thrown when conversion helper is accessed before loading types</exception>
    public PrimitiveConversionHelper PrimitiveConversions => PrimitiveConversionsBackingField ?? throw new Exception("Conversion helper accessed before loading types");

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

        CoreTypesBackingField = new CoreTypeHelper {
            Null = new TypeIdentifier(TypeInfo.Null, []),
            I8 = GetFromType<Runtime.Core.I8>(),
            I16 = GetFromType<Runtime.Core.I16>(),
            I32 = GetFromType<Runtime.Core.I32>(),
            I64 = GetFromType<Runtime.Core.I64>(),
            I128 = GetFromType<Runtime.Core.I128>(),
            U8 = GetFromType<Runtime.Core.U8>(),
            U16 = GetFromType<Runtime.Core.U16>(),
            U32 = GetFromType<Runtime.Core.U32>(),
            U64 = GetFromType<Runtime.Core.U64>(),
            U128 = GetFromType<Runtime.Core.U128>(),
            F16 = GetFromType<Runtime.Core.F16>(),
            F32 = GetFromType<Runtime.Core.F32>(),
            F64 = GetFromType<Runtime.Core.F64>(),
            Bool = GetFromType<Runtime.Core.Bool>(),
            Char = GetFromType<Runtime.Core.Char>(),
            Str = GetFromType<Runtime.Core.Str>()
        };

        PrimitiveConversionsBackingField = new PrimitiveConversionHelper(CoreTypesBackingField);
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
    private TypeIdentifier GetFromType<T>() {
        Type type = typeof(T);

        if (type.IsGenericType) {
            throw new ArgumentException("Type cannot be generic");
        }

        string name = ClassLoader.GetTypeName(type);

        TypeInfo typeInfo = Types.First(x => x.Name == name);

        return new TypeIdentifier(typeInfo, []);
    }

    public sealed class CoreTypeHelper {
        public required TypeIdentifier Null { get; init; }
        public required TypeIdentifier I8 { get; init; }
        public required TypeIdentifier I16 { get; init; }
        public required TypeIdentifier I32 { get; init; }
        public required TypeIdentifier I64 { get; init; }
        public required TypeIdentifier I128 { get; init; }
        public required TypeIdentifier U8 { get; init; }
        public required TypeIdentifier U16 { get; init; }
        public required TypeIdentifier U32 { get; init; }
        public required TypeIdentifier U64 { get; init; }
        public required TypeIdentifier U128 { get; init; }
        public required TypeIdentifier F16 { get; init; }
        public required TypeIdentifier F32 { get; init; }
        public required TypeIdentifier F64 { get; init; }
        public required TypeIdentifier Bool { get; init; }
        public required TypeIdentifier Char { get; init; }
        public required TypeIdentifier Str { get; init; }
    }

    public sealed class PrimitiveConversionHelper {
        private const byte NONE = 0;
        private const byte EXTEND = 1;
        
        
        private CoreTypeHelper CoreTypeHelper { get; }
        private TypeIdentifier[] PrimitiveTypes { get; }
        private byte[,] ConversionTable { get; }
        
        public PrimitiveConversionHelper(CoreTypeHelper types) {
            CoreTypeHelper = types;
            
            PrimitiveTypes = [
                types.I8,
                types.I16,
                types.I32,
                types.I64,
                types.I128,
                types.U8,
                types.U16,
                types.U32,
                types.U64,
                types.U128,
                types.F16,
                types.F32,
                types.F64,
                types.Bool,
                types.Char
            ];
            
            ConversionTable = new byte[PrimitiveTypes.Length, PrimitiveTypes.Length];
            
            AddConversion(types.I8, [types.I16, types.I32, types.I64, types.I128], EXTEND);
            AddConversion(types.I16, [types.I32, types.I64, types.I128], EXTEND);
            AddConversion(types.I32, [types.I64, types.I128], EXTEND);
            AddConversion(types.I64, [types.I128], EXTEND);
            
            AddConversion(types.U8, [types.I16, types.U16, types.I32, types.U32, types.I64, types.U64, types.I128, types.U128], EXTEND);
            AddConversion(types.U16, [types.I32, types.U32, types.I64, types.U64, types.I128, types.U128], EXTEND);
            AddConversion(types.U32, [types.I64, types.U64, types.I128, types.U128], EXTEND);
            AddConversion(types.U64, [types.I128, types.U128], EXTEND);
        }

        private void AddConversion(TypeIdentifier source, IEnumerable<TypeIdentifier> destination, byte conversionType) {
            int sourceIndex = Array.IndexOf(PrimitiveTypes, source);

            foreach (TypeIdentifier destinationType in destination) {
                int destinationIndex = Array.IndexOf(PrimitiveTypes, destinationType);

                ConversionTable[sourceIndex, destinationIndex] = conversionType;
            }
        }

        /*private TypeIdentifier FindExtendCast(TypeIdentifier source, TypeIdentifier destination) {
            
        }*/
        
        public bool IsPrimitiveType(TypeIdentifier type) {
            if (type == CoreTypeHelper.Null) {
                return true;
            }
            
            foreach (TypeIdentifier t in PrimitiveTypes) {
                if (type == t) {
                    return true;
                }
            }

            return false;
        }

        public bool CommonExtendType(TypeIdentifier source, TypeIdentifier destination) {
            int sourceIndex = Array.IndexOf(PrimitiveTypes, source);
            int destinationIndex = Array.IndexOf(PrimitiveTypes, destination);

            if (sourceIndex < 0 || destinationIndex < 0) {
                return false;
            }

            return ConversionTable[sourceIndex, destinationIndex] == EXTEND;
        }
    }
}