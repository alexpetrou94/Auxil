using System.Text;
using JetBrains.Annotations;
using Auxil.Common.Text;
using Auxil.Extensions;

namespace Auxil.Tests.Extensions;

[TestSubject(typeof(StringBuilderExtensions))]
public class StringBuilderExtensionsTest
{
    [Fact]
    public void PeekBack_ReturnsLastCharByDefault()
    {
        StringBuilder sb = new("hello");

        sb.PeekBack().ShouldBe("o");
    }

    [Fact]
    public void PeekBack_ReturnsLastCountChars()
    {
        StringBuilder sb = new("hello");

        sb.PeekBack(2).ShouldBe("lo");
        sb.PeekBack(5).ShouldBe("hello");
    }

    [Fact]
    public void PeekBack_ReturnsEmptyWhenCountOutOfRange()
    {
        StringBuilder sb = new("hello");

        sb.PeekBack(6).ShouldBe(string.Empty);
        sb.PeekBack(0).ShouldBe(string.Empty);
    }

    [Fact]
    public void PeekBack_ReturnsEmptyForEmptyBuilder()
    {
        StringBuilder sb = new();

        sb.PeekBack().ShouldBe(string.Empty);
    }

    [Fact]
    public void PeekBack_ThrowsOnNull()
    {
        StringBuilder? sb = null;

        Should.Throw<ArgumentNullException>(() => sb!.PeekBack());
    }

    [Fact]
    public void NewLine_AppendsSelectedNewLineCharacter()
    {
        StringBuilder sb = new("hello");

        sb.NewLine(NewLineCharacter.CarriageReturnLineFeed).ToString().ShouldBe("hello\r\n");
    }

    [Fact]
    public void AppendWithNewLine_AppendsValueAndNewLine()
    {
        StringBuilder sb = new("hello");

        sb.AppendWithNewLine("world").ToString().ShouldBe("helloworld\n");
    }

    [Fact]
    public void Tab_AppendsTabs()
    {
        StringBuilder sb = new();

        sb.Tab(3).ToString().ShouldBe("\t\t\t");
    }

    [Fact]
    public void IsEmpty_ReflectsLength()
    {
        StringBuilder sb = new();

        sb.IsEmpty.ShouldBeTrue();
        sb.Append('x').IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void Duplicate_CopiesContent()
    {
        StringBuilder sb = new("hello");

        StringBuilder copy = sb.Duplicate();

        copy.ToString().ShouldBe("hello");
        copy.Append('!').ShouldNotBeSameAs(sb);
    }

    [Fact]
    public void Prepend_InsertsAtStart()
    {
        StringBuilder sb = new("world");

        sb.Prepend("hello ").ToString().ShouldBe("hello world");
    }

    [Theory]
    [InlineData(1, true, "hello")]
    [InlineData(2, true, "hell")]
    [InlineData(3, false, "hell")]
    [InlineData(1, false, "hello\r")]
    [InlineData(6, true, "")]
    [InlineData(0, true, "hello\r\n")]
    [InlineData(8, true, "")]
    [InlineData(8, false, "")]
    public void RemoveLast(int count, bool treatEndCharacterAsOne, string expected)
    {
        StringBuilder sb = new("hello\r\n");

        sb.RemoveLast(count, treatEndCharacterAsOne).ToString().ShouldBe(expected);
    }
}
