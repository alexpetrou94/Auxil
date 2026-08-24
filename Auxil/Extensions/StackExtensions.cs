namespace Auxil.Extensions;

/// <summary>Provides convenience members for working with stacks.</summary>
[PublicAPI]
public static class StackExtensions
{
    /// <summary>Pushes the same item onto the stack the given number of times.</summary>
    /// <param name="stack">The stack to push onto.</param>
    /// <param name="item">The item to push.</param>
    /// <param name="count">The number of times to push the item.</param>
    public static void PushRepeated<T>(this Stack<T> stack, T item, int count)
    {
        ArgumentNullException.ThrowIfNull(stack);

        for (int i = 0; i < count; i++)
        {
            stack.Push(item);
        }
    }
}
