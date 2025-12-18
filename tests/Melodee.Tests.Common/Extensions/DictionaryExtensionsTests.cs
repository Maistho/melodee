using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class DictionaryExtensionsTests
{
    [Fact]
    public void Merge_EmptyDictionaries_ReturnsEmptyDictionary()
    {
        var dictionaries = new List<Dictionary<string, int>>();
        var result = DictionaryExtensions.Merge(dictionaries);
        Assert.Empty(result);
    }

    [Fact]
    public void Merge_SingleDictionary_ReturnsCopy()
    {
        var dict = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var result = DictionaryExtensions.Merge(new[] { dict });

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result["a"]);
        Assert.Equal(2, result["b"]);
    }

    [Fact]
    public void Merge_MultipleDictionaries_MergesAll()
    {
        var dict1 = new Dictionary<string, int> { { "a", 1 } };
        var dict2 = new Dictionary<string, int> { { "b", 2 } };
        var dict3 = new Dictionary<string, int> { { "c", 3 } };

        var result = DictionaryExtensions.Merge(new[] { dict1, dict2, dict3 });

        Assert.Equal(3, result.Count);
        Assert.Equal(1, result["a"]);
        Assert.Equal(2, result["b"]);
        Assert.Equal(3, result["c"]);
    }

    [Fact]
    public void Merge_DuplicateKeys_LastValueWins()
    {
        var dict1 = new Dictionary<string, int> { { "a", 1 } };
        var dict2 = new Dictionary<string, int> { { "a", 2 } };

        var result = DictionaryExtensions.Merge(new[] { dict1, dict2 });

        Assert.Single(result);
        Assert.Equal(2, result["a"]);
    }

    [Fact]
    public void Merge_WithEmptyDictionaryInList_IgnoresEmpty()
    {
        var dict1 = new Dictionary<string, int> { { "a", 1 } };
        var dict2 = new Dictionary<string, int>();
        var dict3 = new Dictionary<string, int> { { "b", 2 } };

        var result = DictionaryExtensions.Merge(new[] { dict1, dict2, dict3 });

        Assert.Equal(2, result.Count);
    }
}
