using Noa.Compiler.Symbols;

namespace Noa.Compiler.Bytecode.Builders;

/// <summary>
/// Creates and manages variables in a function.
/// </summary>
/// <param name="parameterCount">The amount of variable indices to reserve for parameters.</param>
internal sealed class LocalsInator(uint parameterCount, IReadOnlyCollection<IVariableSymbol> captures)
{
    private readonly uint parameterCount = parameterCount;
    private readonly uint capturesCount = (uint)captures.Count;
    private readonly Stack<uint> temporaries = [];
    private readonly Dictionary<IVariableSymbol, VariableIndex> indices = [];
    private uint capturesOffset = 0;
    private uint variableOffset = 0;

    /// <summary>
    /// The amount of "static" locals, i.e. parameters and captures.
    /// </summary>
    public uint StaticLocals => parameterCount + capturesCount;

    /// <summary>
    /// The amount of total locals.
    /// </summary>
    public uint Locals => StaticLocals + variableOffset;

    /// <summary>
    /// The amount of variables created.
    /// </summary>
    public uint Variables => variableOffset;

    /// <summary>
    /// The amount of parameters reserved.
    /// </summary>
    public uint Parameters => parameterCount;

    /// <summary>
    /// The amount of captures reserved.
    /// </summary>
    public uint Captures => capturesCount;
    
    /// <summary>
    /// Gets a temporary variable.
    /// </summary>
    public TemporaryVariable GetTemp()
    {
        var index = temporaries.TryPop(out var x)
            ? x
            : StaticLocals + variableOffset++;
        
        return new(new(index), () => temporaries.Push(index));
    }

    private VariableIndex CreateVariable() => new(StaticLocals + variableOffset++);

    /// <summary>
    /// Gets or creates a new variable index for a variable.
    /// </summary>
    /// <remarks>
    /// Uses the <see cref="ParameterSymbol.ParameterIndex"/> for parameter symbols.
    /// </remarks>
    /// <param name="variable">The variable to get the index for.</param>
    public VariableIndex GetOrCreateVariable(IVariableSymbol variable)
    {
        if (captures.Contains(variable))
        {
            if (indices.TryGetValue(variable, out var captureIndex)) return captureIndex;

            if (capturesOffset >= capturesCount) throw new InvalidOperationException("Too many captures.");

            captureIndex = new(parameterCount + capturesOffset++);
            indices.Add(variable, captureIndex);
            return captureIndex;
        }

        if (variable is ParameterSymbol parameter) return new((uint)parameter.ParameterIndex);

        if (variable is not VariableSymbol) throw new UnreachableException();

        if (indices.TryGetValue(variable, out var index)) return index;
        
        index = CreateVariable();
        indices.Add(variable, index);
        return index;
    }
}

/// <summary>
/// A temporary variable which can be released for re-use.
/// </summary>
/// <param name="variable">The index of the temporary variable.</param>
/// <param name="onReleased">An action to invoke once the temporary variable is released.</param>
internal readonly struct TemporaryVariable(VariableIndex variable, Action onReleased) : IDisposable
{
    /// <summary>
    /// The index of the temporary variable.
    /// </summary>
    public VariableIndex Variable { get; } = variable;
    
    public void Dispose() => onReleased();
}
