namespace Noa.Compiler.Emit;

/// <summary>
/// Tracks the runtime size of the stack.
/// </summary>
public sealed class StackTracker
{
    private readonly Stack<int> states = new([0]);

    /// <summary>
    /// The currently tracked size of the stack.
    /// </summary>
    public int Current
    {
        get => states.Peek();
        set
        {
            states.Pop();
            states.Push(value);
        }
    }

    /// <summary>
    /// Pushes a new tracked stack size.
    /// </summary>
    public void PushNew() => states.Push(0);

    /// <summary>
    /// Pops the currently tracked stack size and returns it.
    /// </summary>
    public int PopCurrent()
    {
        if (states.Count == 1) throw new InvalidOperationException("Cannot pop bottommost tracked stack size.");
        
        return states.Pop();
    }

    /// <summary>
    /// Increments the currently tracked stack size.
    /// </summary>
    public void Increment() => Current += 1;

    /// <summary>
    /// Decrements the currently tracked stack size.
    /// </summary>
    public void Decrement() => Current -= 1;
}
