namespace Runtime.Internal;

/// <summary>
/// Hide a class member from the interpreter
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event)]
public sealed class InternalAttribute: Attribute;