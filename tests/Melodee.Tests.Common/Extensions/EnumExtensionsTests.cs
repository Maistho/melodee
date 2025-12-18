using System.ComponentModel;
using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class EnumExtensionsTests
{
    private enum TestEnum
    {
        [Description("First Value Description")]
        First,
        [Description("Second Value Description")]
        Second,
        NoDescription
    }

    [Fact]
    public void GetEnumDescriptionValue_WithDescription_ReturnsDescription()
    {
        var result = TestEnum.First.GetEnumDescriptionValue();
        Assert.Equal("First Value Description", result);
    }

    [Fact]
    public void GetEnumDescriptionValue_WithoutDescription_ReturnsName()
    {
        var result = TestEnum.NoDescription.GetEnumDescriptionValue();
        Assert.Equal("NoDescription", result);
    }

    [Fact]
    public void GetEnumDescriptionValue_NonEnum_ThrowsInvalidOperationException()
    {
        var notAnEnum = 123;
        Assert.Throws<InvalidOperationException>(() => notAnEnum.GetEnumDescriptionValue());
    }

    [Fact]
    public void GetAttribute_WithAttribute_ReturnsAttribute()
    {
        var result = TestEnum.First.GetAttribute<DescriptionAttribute>();
        Assert.NotNull(result);
        Assert.Equal("First Value Description", result.Description);
    }

    [Fact]
    public void GetAttribute_WithoutAttribute_ReturnsNull()
    {
        var result = TestEnum.NoDescription.GetAttribute<DescriptionAttribute>();
        Assert.Null(result);
    }

    [Fact]
    public void ToDictionary_ReturnsAllValues()
    {
        var result = TestEnum.First.ToDictionary();

        Assert.Equal(3, result.Count);
        Assert.Equal("First", result[0]);
        Assert.Equal("Second", result[1]);
        Assert.Equal("NoDescription", result[2]);
    }

    [Fact]
    public void ToNormalizedDictionary_ReturnsNormalizedValues()
    {
        var result = TestEnum.First.ToNormalizedDictionary();

        Assert.Equal(3, result.Count);
        // ToNormalizedString returns uppercase without spaces/special chars
        Assert.Contains(result.Values, v => v.Contains("FIRST", StringComparison.OrdinalIgnoreCase));
    }
}
