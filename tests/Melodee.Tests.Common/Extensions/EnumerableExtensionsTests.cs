using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class EnumerableExtensionsTests
{
    [Fact]
    public void ToCsv_WithDefaultSeparator_ReturnsCommaSeparated()
    {
        var items = new[] { "a", "b", "c" };
        Assert.Equal("a,b,c", items.ToCsv());
    }

    [Fact]
    public void ToCsv_WithCustomSeparator_ReturnsCustomSeparated()
    {
        var items = new[] { "a", "b", "c" };
        Assert.Equal("a;b;c", items.ToCsv(";"));
    }

    [Fact]
    public void ToCsv_EmptyCollection_ReturnsEmptyString()
    {
        var items = Array.Empty<string>();
        Assert.Equal("", items.ToCsv());
    }

    [Fact]
    public void ToCsv_SingleItem_ReturnsItemOnly()
    {
        var items = new[] { "only" };
        Assert.Equal("only", items.ToCsv());
    }

    [Fact]
    public void ToCsv_WithIntegers_ConvertsToStrings()
    {
        var items = new[] { 1, 2, 3 };
        Assert.Equal("1,2,3", items.ToCsv());
    }

    [Fact]
    public void ToDelimitedList_WithDefaultDelimiter_ReturnsPipeSeparated()
    {
        var items = new[] { "a", "b", "c" };
        Assert.Equal("a|b|c", items.ToDelimitedList());
    }

    [Fact]
    public void ToDelimitedList_WithCustomDelimiter_ReturnsCustomSeparated()
    {
        var items = new[] { "a", "b", "c" };
        Assert.Equal("a-b-c", items.ToDelimitedList('-'));
    }

    [Fact]
    public void ToDelimitedList_NullSource_ThrowsArgumentNullException()
    {
        IEnumerable<string> items = null!;
        Assert.Throws<ArgumentNullException>(() => items.ToDelimitedList());
    }

    [Theory]
    [InlineData(null, true, null)]
    [InlineData(null, false, new string[0])]
    [InlineData("", true, null)]
    [InlineData("", false, new string[0])]
    [InlineData("  ", true, null)]
    [InlineData("  ", false, new string[0])]
    public void FromDelimitedList_NullOrWhitespace_ReturnsExpected(string? input, bool returnNull, string[]? expected)
    {
        var result = input.FromDelimitedList(returnNull);
        if (expected == null)
        {
            Assert.Null(result);
        }
        else
        {
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }

    [Fact]
    public void FromDelimitedList_ValidCsvString_ReturnsTrimmedList()
    {
        var result = "a, b , c".FromDelimitedList();
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(3, list.Count);
        Assert.Equal("a", list[0]);
        Assert.Equal("b", list[1]);
        Assert.Equal("c", list[2]);
    }

    [Fact]
    public void FromDelimitedList_TrailingComma_IsTrimmed()
    {
        var result = "a,b,c,".FromDelimitedList();
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public void ForEach_ExecutesActionWithIndex()
    {
        var items = new[] { "a", "b", "c" };
        var results = new List<(string item, int index)>();

        items.ForEach((item, index) => results.Add((item, index)));

        Assert.Equal(3, results.Count);
        Assert.Equal(("a", 0), results[0]);
        Assert.Equal(("b", 1), results[1]);
        Assert.Equal(("c", 2), results[2]);
    }

    [Fact]
    public void ForEach_EmptyCollection_DoesNothing()
    {
        var items = Array.Empty<string>();
        var count = 0;

        items.ForEach((_, _) => count++);

        Assert.Equal(0, count);
    }

    [Fact]
    public void SelectManyRecursive_WithNestedStructure_FlattensAll()
    {
        var root = new TestNode("root")
        {
            Children =
            [
                new TestNode("child1") { Children = [new TestNode("grandchild1")] },
                new TestNode("child2")
            ]
        };

        var result = new[] { root }.SelectManyRecursive(n => n.Children).ToList();

        Assert.Equal(3, result.Count);
        Assert.Contains(result, n => n.Name == "child1");
        Assert.Contains(result, n => n.Name == "child2");
        Assert.Contains(result, n => n.Name == "grandchild1");
    }

    [Fact]
    public void SelectManyRecursive_NullSource_ReturnsEmpty()
    {
        IEnumerable<TestNode>? source = null;
        var result = source.SelectManyRecursive(n => n.Children);
        Assert.Empty(result);
    }

    [Fact]
    public void SelectManyRecursive_NoChildren_ReturnsEmpty()
    {
        var items = new[] { new TestNode("leaf") };
        var result = items.SelectManyRecursive(n => n.Children);
        Assert.Empty(result);
    }

    private class TestNode
    {
        public string Name { get; }
        public IEnumerable<TestNode> Children { get; set; } = [];

        public TestNode(string name) => Name = name;
    }
}
