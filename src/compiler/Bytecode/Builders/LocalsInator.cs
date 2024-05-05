namespace Noa.Compiler.Bytecode.Builders;

/// <summary>
/// Creates and manages variables in a function.
/// </summary>
/// <param name="parameterCount">The amount of variable indices to reserve for parameters.</param>
internal sealed class LocalsInator(uint parameterCount)
{
    private readonly uint parameterCount = parameterCount;
    private readonly Stack<uint> temporaries = [];
    private uint currentIndex = parameterCount;

    /// <summary>
    /// The amount of total locals.
    /// </summary>
    public uint Locals => currentIndex;

    /// <summary>
    /// The amount of variables created.
    /// </summary>
    public uint Variables => currentIndex - parameterCount;

    /// <summary>
    /// The amount of parameters reserved.
    /// </summary>
    public uint Parameters => parameterCount;
    
    /// <summary>
    /// Gets a temporary variable.
    /// </summary>
    public TemporaryVariable GetTemp()
    {
        var index = temporaries.TryPop(out var x)
            ? x
            : currentIndex++;
        
        return new(new(index), () => temporaries.Push(index));
    }

    /// <summary>
    /// Creates a new variable.
    /// </summary>
    public VariableIndex CreateVariable() => new(currentIndex++);

    /// <summary>
    /// Gets the variable for a parameter.
    /// </summary>
    /// <param name="parameterIndex">The index of the parameter to get the variable for.</param>
    /// <returns></returns>
    public VariableIndex GetParameterVariable(int parameterIndex) => new((uint)parameterIndex);
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
