namespace Auxil.Attributes;

/// <summary>Associates a display or serialization string with an enum field.</summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Field)]
public sealed class StringValueAttribute : Attribute
{
    /// <summary>The string value associated with the enum field.</summary>
    public string StringValue { get; }

    /// <summary>Creates a new <see cref="StringValueAttribute"/> with the given value.</summary>
    /// <param name="value">The string value to associate with the enum field.</param>
    public StringValueAttribute(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        StringValue = value;
    }
}
