namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Handlers;
using Grammar;
using Interpreter.Serialization;
using Interpreter.Bytecode;
using Microsoft.Extensions.Logging;

internal sealed class Preprocessor(CompilerOptions options, ILogger logger): ScrantonBaseVisitor<object?> {
    private CompilerOptions Options { get; } = options;
    private ILogger Logger { get; } = logger;
}