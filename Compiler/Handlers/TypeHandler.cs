namespace Compiler.Handlers;

using Data;
using Interpreter;
using Interpreter.Types;

/// <summary>
/// This class manages the imported types the program sees.
/// </summary>
internal sealed class TypeHandler {
    /// <summary>
    /// Whether all runtime code should be visible by default.
    /// </summary>
    private bool AutoImportEnabled { get; set; }

    /// <summary>
    /// The imported modules of the program.
    /// </summary>
    private HashSet<string> Imports { get; }

    /// <summary>
    /// The list of currently visible types.
    /// </summary>
    private List<TypeDefinition> Types { get; }

    /// <summary>
    /// The module of the current program.
    /// </summary>
    private string? ProgramModule { get; set; }

    /// <summary>
    /// The address of the program module name in the data section.
    /// </summary>
    public int ModuleNameAddress { get; private set; } = -1;

    /// <summary>
    /// Helper to retrieve type identifiers for core types.
    /// </summary>
    public CoreTypeHelper CoreTypes { get; }

    /// <summary>
    /// Helper for type conversions.
    /// </summary>
    public CastHelper Casts { get; }

    /// <summary>
    /// Create a new type handler.
    /// </summary>
    public TypeHandler() {
        // load initial types
        ClassLoader.LoadRuntimeInitial(out HashSet<string> imports, out List<TypeDefinition> types);

        // assign properties
        Imports = imports;
        Types = types;
        CoreTypes = new CoreTypeHelper(Types);
        Casts = new CastHelper(CoreTypes);
    }

    /// <summary>
    /// Set the module of the program.
    /// </summary>
    /// <param name="address">The address of the string in the data section.</param>
    /// <param name="name">The module to use.</param>
    public void SetModule(int address, string name) {
        // already set
        if (ProgramModule is not null) {
            return;
        }

        // set address and name
        ModuleNameAddress = address;
        ProgramModule = name;

        // add module to imports
        // do not emit a warning if it's a duplicate
        AddImport(ProgramModule);
    }

    /// <summary>
    /// Import a module.
    /// </summary>
    /// <param name="name">The name of the module.</param>
    /// <returns>False if the module was already imported, true otherwise.</returns>
    public bool AddImport(string name) {
        return Imports.Add(name);
    }

    /// <summary>
    /// Enable auto imports.
    /// </summary>
    /// <returns>False if it was already enabled, true otherwise.</returns>
    public bool EnableAutoImport() {
        bool success = !AutoImportEnabled;

        AutoImportEnabled = true;

        return success;
    }

    /// <summary>
    /// Load all currently visible types.
    /// </summary>
    public void LoadTypes() {
        ClassLoader.LoadRuntime(AutoImportEnabled, Imports, Types);
    }

    /// <summary>
    /// Retrieve a type by name.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <returns>The type if it was found, null otherwise.</returns>
    public TypeDefinition? GetTypeByName(string name) {
        return Types.FirstOrDefault(type => type.Name == name);
    }

    /// <summary>
    /// Collection of properties that represent all types constants can have.
    /// </summary>
    internal sealed class CoreTypeHelper {
        /// <summary>
        /// Identifier for 8-bit signed integers.
        /// </summary>
        public TypeIdentifier I8 { get; }

        /// <summary>
        /// Identifier for 16-bit signed integers.
        /// </summary>
        public TypeIdentifier I16 { get; }

        /// <summary>
        /// Identifier for 32-bit signed integers.
        /// </summary>
        public TypeIdentifier I32 { get; }

        /// <summary>
        /// Identifier for 64-bit signed integers.
        /// </summary>
        public TypeIdentifier I64 { get; }

        /// <summary>
        /// Identifier for 128-bit signed integers.
        /// </summary>
        public TypeIdentifier I128 { get; }

        /// <summary>
        /// Identifier for 8-bit unsigned integers.
        /// </summary>
        public TypeIdentifier U8 { get; }

        /// <summary>
        /// Identifier for 16-bit unsigned integers.
        /// </summary>
        public TypeIdentifier U16 { get; }

        /// <summary>
        /// Identifier for 32-bit unsigned integers.
        /// </summary>
        public TypeIdentifier U32 { get; }

        /// <summary>
        /// Identifier for 64-bit unsigned integers.
        /// </summary>
        public TypeIdentifier U64 { get; }

        /// <summary>
        /// Identifier for 128-bit unsigned integers.
        /// </summary>
        public TypeIdentifier U128 { get; }

        /// <summary>
        /// Identifier for 16-bit floats.
        /// </summary>
        public TypeIdentifier F16 { get; }

        /// <summary>
        /// Identifier for 32-bit floats.
        /// </summary>
        public TypeIdentifier F32 { get; }

        /// <summary>
        /// Identifier for 64-bit floats.
        /// </summary>
        public TypeIdentifier F64 { get; }

        /// <summary>
        /// Identifier for booleans.
        /// </summary>
        public TypeIdentifier Bool { get; }

        /// <summary>
        /// Identifier for characters.
        /// </summary>
        public TypeIdentifier Char { get; }

        /// <summary>
        /// Identifier for strings.
        /// </summary>
        public TypeIdentifier Str { get; }

        /// <summary>
        /// Identifier for a return type that does not exist.
        /// </summary>
        public TypeIdentifier Void { get; }
        
        /// <summary>
        /// The type of a null reference.
        /// </summary>
        /// <remarks>
        /// Assignable to any reference type. Equivalent to a void pointer.
        /// </remarks>
        public TypeIdentifier Null { get; }

        /// <summary>
        /// Create a new core type helper.
        /// </summary>
        /// <param name="types">A list of type definitions that contain all the core types.</param>
        public CoreTypeHelper(List<TypeDefinition> types) {
            I8 = GetCoreType<Runtime.Core.I8>();
            I16 = GetCoreType<Runtime.Core.I16>();
            I32 = GetCoreType<Runtime.Core.I32>();
            I64 = GetCoreType<Runtime.Core.I64>();
            I128 = GetCoreType<Runtime.Core.I128>();
            U8 = GetCoreType<Runtime.Core.U8>();
            U16 = GetCoreType<Runtime.Core.U16>();
            U32 = GetCoreType<Runtime.Core.U32>();
            U64 = GetCoreType<Runtime.Core.U64>();
            U128 = GetCoreType<Runtime.Core.U128>();
            F16 = GetCoreType<Runtime.Core.F16>();
            F32 = GetCoreType<Runtime.Core.F32>();
            F64 = GetCoreType<Runtime.Core.F64>();
            Bool = GetCoreType<Runtime.Core.Bool>();
            Char = GetCoreType<Runtime.Core.Char>();
            Str = GetCoreType<Runtime.Core.Str>();

            Void = new TypeIdentifier(new TypeDefinition {
                Modifiers = default,
                IsReference = false,
                Name = "void",
                GenericIndex = -1,
                Fields = [],
                Methods = [],
                GenericParameterCount = 0,
                StackSize = 0
            }, []);
            
            Null = new TypeIdentifier(new TypeDefinition {
                Modifiers = default,
                IsReference = true,
                Name = "null",
                GenericIndex = -1,
                Fields = [],
                Methods = [],
                GenericParameterCount = 0,
                StackSize = 8
            }, []);

            return;

            TypeIdentifier GetCoreType<T>() {
                string name = ClassLoader.GetTypeName(typeof(T));

                TypeDefinition typeDefinition = types.First(x => x.Name == name);

                return new TypeIdentifier(typeDefinition, []);
            }
        }
    }

    /// <summary>
    /// Helper class for type conversions.
    /// </summary>
    internal sealed class CastHelper {
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
                if (signedType.Size > 8) continue;

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
                if (unsignedType.Size > 8) continue;

                foreach (TypeIdentifier floatType in FloatTypes) {
                    AddCast(unsignedType, floatType, PrimitiveCast.UnsignedToFloatImplicit);
                }
            }

            AddCast(types.U16, types.Char, PrimitiveCast.NotRequired);

            // float
            foreach (TypeIdentifier floatType in FloatTypes) {
                foreach (TypeIdentifier signedType in SignedIntegerTypes) {
                    if (signedType.Size > 8) continue;
                    AddCast(floatType, signedType, PrimitiveCast.FloatToSignedExplicit);
                }

                foreach (TypeIdentifier unsignedType in UnsignedIntegerTypes) {
                    if (unsignedType.Size > 8) continue;
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

        /// <summary>
        /// Checks if a type is a primitive integer type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is an integer, false otherwise.</returns>
        public bool IsIntegerType(TypeIdentifier type) {
            return SignedIntegerTypes.Contains(type) || UnsignedIntegerTypes.Contains(type);
        }

        /// <summary>
        /// Checks if a type is a primitive float type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is a float, false otherwise.</returns>
        public bool IsFloatType(TypeIdentifier type) {
            return FloatTypes.Contains(type);
        }

        /// <summary>
        /// Retrieves the cast type between 2 primitive types.
        /// </summary>
        /// <param name="sourceType">The type to cast from.</param>
        /// <param name="targetType">The type to cast to.</param>
        /// <returns>The type of the cast. If the supplied types are not primitive, returns the invalid cast value.</returns>
        public PrimitiveCast GetPrimitiveCast(TypeIdentifier sourceType, TypeIdentifier targetType) {
            int sourceIndex = Array.IndexOf(PrimitiveTypes, sourceType);
            int destinationIndex = Array.IndexOf(PrimitiveTypes, targetType);

            if (sourceIndex < 0 || destinationIndex < 0) {
                return PrimitiveCast.None;
            }

            return ConversionTable[sourceIndex, destinationIndex];
        }
    }
}