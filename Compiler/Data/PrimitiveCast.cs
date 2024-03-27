namespace Compiler.Data;

internal enum PrimitiveCast : byte {
    None,
    FloatToFloatExplicit,
    FloatToUnsignedExplicit,
    FloatToSignedExplicit,
    ResizeExplicit,
    FloatToFloatImplicit,
    UnsignedToFloatImplicit,
    SignedToFloatImplicit,
    ResizeImplicit,
    NotRequired,
}