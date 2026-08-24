using System.Xml;
using JetBrains.Annotations;
using Auxil.Extensions;

namespace Auxil.Tests.Extensions;

[TestSubject(typeof(XmlExtensions))]
public class XmlExtensionsTest
{
    [Fact]
    public void GetAttributeValue_ReturnsAttributeValue()
    {
        XmlElement element = CreateElement();

        element.GetAttributeValue("name").ShouldBe("value");
    }

    [Fact]
    public void GetAttributeValue_ReturnsNullForMissingAttribute()
    {
        XmlElement element = CreateElement();

        element.GetAttributeValue("missing").ShouldBeNull();
    }

    [Fact]
    public void GetAttributeValue_ReturnsNullForElementWithoutAttributes()
    {
        XmlDocument document = new();
        XmlElement element = document.CreateElement("root");

        element.GetAttributeValue("anything").ShouldBeNull();
    }

    private static XmlElement CreateElement()
    {
        XmlDocument document = new();
        XmlElement element = document.CreateElement("root");
        element.SetAttribute("name", "value");

        return element;
    }
}
