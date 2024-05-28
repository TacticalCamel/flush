namespace Compiler.Data;

/// <summary>
/// Represents a method of converting one primitive type to another.
/// The enum values are ordered by preference. When multiple conversions are possible,
/// the one with a higher value is preferred.
/// </summary>
internal enum PrimitiveCast : byte {
    /// <summary>
    /// Conversion is not possible.
    /// </summary>
    None,

    /// <summary>
    /// Reinterpret a floating point number to a different precision.
    /// Precision might be lost, so an explicit cast is required.
    /// </summary>
    FloatToFloatExplicit,

    /// <summary>
    /// Convert a floating point number to an unsigned integer.
    /// An explicit cast is required.
    /// </summary>
    FloatToUnsignedExplicit,

    /// <summary>
    /// Convert a floating point number to a signed integer.
    /// An explicit cast is required.
    /// </summary>
    FloatToSignedExplicit,

    /// <summary>
    /// Change the size of the type. Overflow is possible,
    /// so an explicit cast is required.
    /// </summary>
    ResizeExplicit,

    /// <summary>
    /// Reinterpret a floating point number to a different precision.
    /// No precision is lost.
    /// </summary>
    FloatToFloatImplicit,

    /// <summary>
    /// Convert an unsigned integer to a floating point number.
    /// </summary>
    UnsignedToFloatImplicit,

    /// <summary>
    /// Convert a signed integer to a floating point number.
    /// </summary>
    SignedToFloatImplicit,

    /// <summary>
    /// Change the size of the type.
    /// Overflow is not possible.
    /// </summary>
    ResizeImplicit,

    /// <summary>
    /// No conversion is required.
    /// The value can be interpreted without change.
    /// </summary>
    NotRequired
}

internal static class CastEnumExtensions {
    public static bool IsImplicit(this PrimitiveCast cast) {
        return cast switch {
            PrimitiveCast.FloatToFloatImplicit => true,
            PrimitiveCast.UnsignedToFloatImplicit => true,
            PrimitiveCast.SignedToFloatImplicit => true,
            PrimitiveCast.ResizeImplicit => true,
            PrimitiveCast.NotRequired => true,
            _ => false
        };
    }
    
    public static bool IsExplicit(this PrimitiveCast cast) {
        return cast switch {
            PrimitiveCast.None => true,
            PrimitiveCast.FloatToFloatExplicit => true,
            PrimitiveCast.FloatToUnsignedExplicit => true,
            PrimitiveCast.FloatToSignedExplicit => true,
            PrimitiveCast.ResizeExplicit => true,
            _ => false
        };
    }
}