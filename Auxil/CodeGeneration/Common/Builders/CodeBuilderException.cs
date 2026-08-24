namespace Auxil.CodeGeneration.Common.Builders;

/// <summary>
/// Thrown when the CodeBuilder is used incorrectly: attaching a node
/// to an invalid scope, closing the wrong scope, calling File() twice,
/// rendering before the file is complete, etc.
/// </summary>
[PublicAPI]
public sealed class CodeBuilderException : InvalidOperationException
{
    /// <summary>Creates a new builder exception with the given message.</summary>
    /// <param name="message">A description of the builder misuse.</param>
    public CodeBuilderException(string message) : base(message)
    {
    }
}
