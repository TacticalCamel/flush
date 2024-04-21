namespace Compiler.Handlers;

using static Grammar.ScrantonParser;

internal sealed class ContextHandler {
    /// <summary>
    /// Indicates that preprocessor mode is enabled.
    /// Visit methods may change behaviour depending on this value.
    /// </summary>
    public bool IsPreprocessorMode { get; set; }
    
    public string[]? GenericParameterNames { get; set; }
    
    public TypeDefinitionContext[]? ProcessedTypes { get; set; }
}