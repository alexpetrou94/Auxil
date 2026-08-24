using System.Xml;

namespace Auxil.Extensions;

/// <summary>Provides convenience members for working with XML elements.</summary>
[PublicAPI]
public static class XmlExtensions
{
    /// <summary>Returns the value of the named attribute, or <c>null</c> when it is missing.</summary>
    /// <param name="element">The element to inspect.</param>
    /// <param name="attribute">The attribute name.</param>
    public static string? GetAttributeValue(this XmlElement element, string attribute)
        => element.Attributes.GetNamedItem(attribute)?.Value;
}
