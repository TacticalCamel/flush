namespace Compiler.Handlers;

using Data;
using Interpreter.Bytecode;
using Interpreter.Types;

/// <summary>
/// This class manages the imported types the program sees.
/// To avoid naming conflicts, not all modules are visible by default.
/// </summary>
internal sealed class TypeHandler {
    /// <summary>
    /// Backing field for the core type helper so the public property can have a non-nullable type.
    /// </summary>
    private CoreTypeHelper? CoreTypesBackingField;

    /// <summary>
    /// Helper to retrieve type identifiers for core types.
    /// </summary>
    /// <exception cref="Exception">Thrown when the property is accessed before loading types.</exception>
    public CoreTypeHelper CoreTypes => CoreTypesBackingField ?? throw new Exception("Core type helper accessed before loading types");

    /// <summary>
    /// Backing field for the cast helper so the public property can have a non-nullable type.
    /// </summary>
    private CastHelper? CastsBackingField;

    /// <summary>
    /// Helper for type conversions.
    /// </summary>
    /// <exception cref="Exception">Thrown when the property is accessed before loading types.</exception>
    public CastHelper Casts => CastsBackingField ?? throw new Exception("Conversion helper accessed before loading types");

    /// <summary>
    /// The module of the current program.
    /// Code within the same module is visible by default.
    /// Can be null, in which case the code can not be imported to other programs.
    /// </summary>
    private string? ProgramModule { get; set; }

    /// <summary>
    /// Whether all runtime code should be visible by default.
    /// </summary>
    private bool AutoImportEnabled { get; set; }

    /// <summary>
    /// The imported modules of the program.
    /// Use a hashset to avoid duplicates.
    /// </summary>
    private HashSet<string> Imports { get; } = [];

    /// <summary>
    /// The list of currently visible types.
    /// </summary>
    private List<TypeInfo> Types { get; set; } = [];

    /// <summary>
    /// Gets the modules that should be visible from the current program.
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
    /// Load all visible types.
    /// </summary>
    public void LoadTypes() {
        string[] visibleModules = GetVisibleModules();

        Types = ClassLoader.LoadModules(visibleModules, AutoImportEnabled);

        CoreTypesBackingField = new CoreTypeHelper {
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
            Str = GetFromType<Runtime.Core.Str>(),
            Null = new TypeIdentifier(TypeInfo.Null, [])
        };

        CastsBackingField = new CastHelper(CoreTypesBackingField);
    }

    /// <summary>
    /// Add a new import.
    /// </summary>
    /// <param name="name">The module to import.</param>
    /// <returns>True if successful, false if the imported module was already present.</returns>
    public bool Add(string name) {
        return Imports.Add(name);
    }

    /// <summary>
    /// Enable auto import.
    /// </summary>
    /// <returns>True if successful, false if auto import was already enabled.</returns>
    public bool EnableAutoImport() {
        bool success = !AutoImportEnabled;

        AutoImportEnabled = true;

        return success;
    }

    /// <summary>
    /// Set the module of the program.
    /// </summary>
    /// <param name="name">The module to use.</param>
    public void SetModule(string name) {
        ProgramModule = name;
    }

    /// <summary>
    /// Retrieve a type by name.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <returns>The type if it was found, null otherwise.</returns>
    public TypeInfo? TryGetByName(string name) {
        return Types.FirstOrDefault(type => type.Name == name);
    }

    /// <summary>
    /// Retrieve a non-generic type directly.
    /// </summary>
    /// <typeparam name="T">The the type to retrieve.</typeparam>
    /// <returns>The identifier pf the type.</returns>
    private TypeIdentifier GetFromType<T>() {
        Type type = typeof(T);

        if (type.IsGenericType) {
            throw new ArgumentException("Type cannot be generic");
        }

        string name = ClassLoader.GetTypeName(type);

        TypeInfo typeInfo = Types.First(x => x.Name == name);

        return new TypeIdentifier(typeInfo, []);
    }

    /// <summary>
    /// Collection of properties that represent all types constants can have.
    /// </summary>
    public sealed class CoreTypeHelper {
        /// <summary>
        /// Identifier for null references.
        /// </summary>
        public required TypeIdentifier Null { get; init; }

        /// <summary>
        /// Identifier for 8-bit signed integers.
        /// </summary>
        public required TypeIdentifier I8 { get; init; }

        /// <summary>
        /// Identifier for 16-bit signed integers.
        /// </summary>
        public required TypeIdentifier I16 { get; init; }

        /// <summary>
        /// Identifier for 32-bit signed integers.
        /// </summary>
        public required TypeIdentifier I32 { get; init; }

        /// <summary>
        /// Identifier for 64-bit signed integers.
        /// </summary>
        public required TypeIdentifier I64 { get; init; }

        /// <summary>
        /// Identifier for 128-bit signed integers.
        /// </summary>
        public required TypeIdentifier I128 { get; init; }

        /// <summary>
        /// Identifier for 8-bit unsigned integers.
        /// </summary>
        public required TypeIdentifier U8 { get; init; }

        /// <summary>
        /// Identifier for 16-bit unsigned integers.
        /// </summary>
        public required TypeIdentifier U16 { get; init; }

        /// <summary>
        /// Identifier for 32-bit unsigned integers.
        /// </summary>
        public required TypeIdentifier U32 { get; init; }

        /// <summary>
        /// Identifier for 64-bit unsigned integers.
        /// </summary>
        public required TypeIdentifier U64 { get; init; }

        /// <summary>
        /// Identifier for 128-bit unsigned integers.
        /// </summary>
        public required TypeIdentifier U128 { get; init; }

        /// <summary>
        /// Identifier for 16-bit floats.
        /// </summary>
        public required TypeIdentifier F16 { get; init; }

        /// <summary>
        /// Identifier for 32-bit floats.
        /// </summary>
        public required TypeIdentifier F32 { get; init; }

        /// <summary>
        /// Identifier for 64-bit floats.
        /// </summary>
        public required TypeIdentifier F64 { get; init; }

        /// <summary>
        /// Identifier for booleans.
        /// </summary>
        public required TypeIdentifier Bool { get; init; }

        /// <summary>
        /// Identifier for characters.
        /// </summary>
        public required TypeIdentifier Char { get; init; }

        /// <summary>
        /// Identifier for strings.
        /// </summary>
        public required TypeIdentifier Str { get; init; }
    }

    // TODO comment
    public sealed class CastHelper {
        /// <summary>
        /// A collection of all primitive types.
        /// </summary>
        private TypeIdentifier[] PrimitiveTypes { get; }
        
        /// <summary>
        /// A collection of all signed integer types.
        /// </summary>
        private TypeIdentifier[] SignedIntegerTypes { get; }
        
        /// <summary>
        /// A collection of all unsigned integer types.
        /// </summary>
        private TypeIdentifier[] UnsignedIntegerTypes { get; }
        
        /// <summary>
        /// A collection of all floating point types.
        /// </summary>
        private TypeIdentifier[] FloatTypes { get; }
        
        /// <summary>
        /// A 2-dimensional array to access conversion between primitive types.
        /// </summary>
        private PrimitiveCast[,] ConversionTable { get; }

        /// <summary>
        /// Creates a new cast helper from a core type helper.
        /// </summary>
        /// <param name="types">The core type helper to use.</param>
        public CastHelper(CoreTypeHelper types) {
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

            SignedIntegerTypes = [
                types.I8,
                types.I16,
                types.I32,
                types.I64,
                types.I128
            ];

            UnsignedIntegerTypes = [
                types.U8,
                types.U16,
                types.U32,
                types.U64,
                types.U128
            ];

            FloatTypes = [
                types.F16,
                types.F32,
                types.F64
            ];

            ConversionTable = new PrimitiveCast[PrimitiveTypes.Length, PrimitiveTypes.Length];

            RegisterCasts(types);
        }

        /// <summary>
        /// Initializes all type casts between primitive types.
        /// </summary>
        /// <param name="types">The core type helper to use.</param>
        private void RegisterCasts(CoreTypeHelper types) {
            // signed int
            for (int sourceIndex = 0; sourceIndex < SignedIntegerTypes.Length; sourceIndex++) {
                for (int destinationIndex = 0; destinationIndex < SignedIntegerTypes.Length; destinationIndex++) {
                    PrimitiveCast cast = PrimitiveCast.NotRequired;

                    if (sourceIndex < destinationIndex) cast = PrimitiveCast.ResizeImplicit;
                    else if (sourceIndex > destinationIndex) cast = PrimitiveCast.ResizeExplicit;

                    AddCast(SignedIntegerTypes[sourceIndex], SignedIntegerTypes[destinationIndex], cast);
                }
            }

            foreach (TypeIdentifier signedType in SignedIntegerTypes) {
                foreach (TypeIdentifier unsignedType in UnsignedIntegerTypes) {
                    AddCast(signedType, unsignedType, PrimitiveCast.ResizeExplicit);
                }
            }

            foreach (TypeIdentifier signedType in SignedIntegerTypes) {
                foreach (TypeIdentifier floatType in FloatTypes) {
                    AddCast(signedType, floatType, PrimitiveCast.SignedToFloatImplicit);
                }
            }

            // unsigned int
            for (int sourceIndex = 0; sourceIndex < UnsignedIntegerTypes.Length; sourceIndex++) {
                for (int destinationIndex = 0; destinationIndex < UnsignedIntegerTypes.Length; destinationIndex++) {
                    PrimitiveCast cast = PrimitiveCast.NotRequired;

                    if (sourceIndex < destinationIndex) cast = PrimitiveCast.ResizeImplicit;
                    else if (sourceIndex > destinationIndex) cast = PrimitiveCast.ResizeExplicit;

                    AddCast(UnsignedIntegerTypes[sourceIndex], UnsignedIntegerTypes[destinationIndex], cast);
                }
            }

            for (int sourceIndex = 0; sourceIndex < UnsignedIntegerTypes.Length; sourceIndex++) {
                for (int destinationIndex = 0; destinationIndex < UnsignedIntegerTypes.Length; destinationIndex++) {
                    PrimitiveCast cast = sourceIndex >= destinationIndex ? PrimitiveCast.ResizeExplicit : PrimitiveCast.ResizeImplicit;

                    AddCast(UnsignedIntegerTypes[sourceIndex], SignedIntegerTypes[destinationIndex], cast);
                }
            }

            foreach (TypeIdentifier unsignedType in UnsignedIntegerTypes) {
                foreach (TypeIdentifier floatType in FloatTypes) {
                    AddCast(unsignedType, floatType, PrimitiveCast.UnsignedToFloatImplicit);
                }
            }

            AddCast(types.U16, types.Char, PrimitiveCast.NotRequired);

            // float
            foreach (TypeIdentifier floatType in FloatTypes) {
                foreach (TypeIdentifier signedType in SignedIntegerTypes) {
                    AddCast(floatType, signedType, PrimitiveCast.FloatToSignedExplicit);
                }

                foreach (TypeIdentifier unsignedType in UnsignedIntegerTypes) {
                    AddCast(floatType, unsignedType, PrimitiveCast.FloatToUnsignedExplicit);
                }
            }

            for (int sourceIndex = 0; sourceIndex < FloatTypes.Length; sourceIndex++) {
                for (int destinationIndex = 0; destinationIndex < FloatTypes.Length; destinationIndex++) {
                    PrimitiveCast cast = PrimitiveCast.NotRequired;

                    if (sourceIndex < destinationIndex) cast = PrimitiveCast.FloatToFloatImplicit;
                    else if (sourceIndex > destinationIndex) cast = PrimitiveCast.FloatToFloatExplicit;

                    AddCast(FloatTypes[sourceIndex], FloatTypes[destinationIndex], cast);
                }
            }

            // char
            AddCast(types.Char, types.Char, PrimitiveCast.NotRequired);
            AddCast(types.Char, types.U16, PrimitiveCast.NotRequired);

            // bool
            AddCast(types.Bool, types.Bool, PrimitiveCast.NotRequired);

            return;

            // add a cast between 2 types
            void AddCast(TypeIdentifier sourceType, TypeIdentifier destinationType, PrimitiveCast cast) {
                int sourceIndex = Array.IndexOf(PrimitiveTypes, sourceType);
                int destinationIndex = Array.IndexOf(PrimitiveTypes, destinationType);

                ConversionTable[sourceIndex, destinationIndex] = cast;
            }
        }

        /// <summary>
        /// Checks if a type is primitive.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is primitive, false otherwise.</returns>
        public bool IsPrimitiveType(TypeIdentifier type) {
            foreach (TypeIdentifier primitiveType in PrimitiveTypes) {
                if (type == primitiveType) {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Checks if the given types are primitive.
        /// </summary>
        /// <param name="first">The first type to check.</param>
        /// <param name="second">The second type to check.</param>
        /// <returns>True if both types are primitive, false otherwise.</returns>
        public bool ArePrimitiveTypes(TypeIdentifier first, TypeIdentifier second) {
            return IsPrimitiveType(first) && IsPrimitiveType(second);
        }
        
        public Instruction? GetCastInstruction(TypeIdentifier sourceType, TypeIdentifier targetType) {
            PrimitiveCast cast = GetCast(sourceType, targetType);

            switch(cast) {
                case PrimitiveCast.ResizeImplicit: {
                    int difference = targetType.Size - sourceType.Size;

                    if (difference > 0) {
                         return new Instruction {
                            Code = OperationCode.pshz,
                            Size = difference
                        };
                    }
                    else {
                        return new Instruction {
                            Code = OperationCode.pop,
                            Size = -difference
                        };
                    }
                }
                default:
                    break;
            }

            return null;
        }
        
        public PrimitiveCast GetCast(TypeIdentifier sourceType, TypeIdentifier targetType) {
            int sourceIndex = Array.IndexOf(PrimitiveTypes, sourceType);
            int destinationIndex = Array.IndexOf(PrimitiveTypes, targetType);

            if (sourceIndex < 0 || destinationIndex < 0) {
                return PrimitiveCast.None;
            }

            return ConversionTable[sourceIndex, destinationIndex];
        }
    }
}