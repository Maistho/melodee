using Melodee.Common.Filtering;

namespace Melodee.Tests.Common.Filtering;

public class FilterOperatorInfoTests
{
    [Theory]
    [InlineData(FilterOperator.None, "=")]
    [InlineData(FilterOperator.Equals, "=")]
    [InlineData(FilterOperator.NotEquals, "!=")]
    [InlineData(FilterOperator.LessThan, "<")]
    [InlineData(FilterOperator.LessThanOrEquals, "<=")]
    [InlineData(FilterOperator.GreaterThan, ">")]
    [InlineData(FilterOperator.GreaterThanOrEquals, ">=")]
    [InlineData(FilterOperator.Contains, "LIKE")]
    [InlineData(FilterOperator.StartsWith, "LIKE")]
    [InlineData(FilterOperator.EndsWith, "LIKE")]
    [InlineData(FilterOperator.DoesNotContain, "NOT LIKE")]
    [InlineData(FilterOperator.IsNull, "IS NULL")]
    [InlineData(FilterOperator.IsNotNull, "IS NOT NULL")]
    public void FilterOperatorToConditionString_ReturnsExpectedString(FilterOperator op, string expected)
    {
        Assert.Equal(expected, FilterOperatorInfo.FilterOperatorToConditionString(op));
    }

    [Theory]
    [InlineData(FilterOperator.Contains, true)]
    [InlineData(FilterOperator.StartsWith, true)]
    [InlineData(FilterOperator.EndsWith, true)]
    [InlineData(FilterOperator.DoesNotContain, true)]
    [InlineData(FilterOperator.Equals, false)]
    [InlineData(FilterOperator.NotEquals, false)]
    [InlineData(FilterOperator.LessThan, false)]
    [InlineData(FilterOperator.IsNull, false)]
    public void IsLikeOperator_ReturnsExpectedResult(FilterOperator op, bool expected)
    {
        Assert.Equal(expected, FilterOperatorInfo.IsLikeOperator(op));
    }

    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var filter = new FilterOperatorInfo("Name", FilterOperator.Equals, "Test");

        Assert.Equal("Name", filter.PropertyName);
        Assert.Equal(FilterOperator.Equals, filter.Operator);
        Assert.Equal("Test", filter.Value);
        Assert.Equal("AND", filter.JoinOperator);
        Assert.Null(filter.ColumnName);
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsAllProperties()
    {
        var filter = new FilterOperatorInfo("Name", FilterOperator.Contains, "Test", "OR", "name_column", "ILIKE");

        Assert.Equal("Name", filter.PropertyName);
        Assert.Equal(FilterOperator.Contains, filter.Operator);
        Assert.Equal("Test", filter.Value);
        Assert.Equal("OR", filter.JoinOperator);
        Assert.Equal("name_column", filter.ColumnName);
        Assert.Equal("ILIKE", filter.OperatorOverride);
    }

    [Fact]
    public void OperatorValue_ReturnsConditionString()
    {
        var filter = new FilterOperatorInfo("Name", FilterOperator.Contains, "Test");
        Assert.Equal("LIKE", filter.OperatorValue);
    }

    [Fact]
    public void ValuePattern_ContainsOperator_AddsSurroundingWildcards()
    {
        var filter = new FilterOperatorInfo("Name", FilterOperator.Contains, "Test");
        Assert.Equal("%Test%", filter.ValuePattern());
    }

    [Fact]
    public void ValuePattern_StartsWithOperator_AddsLeadingWildcard()
    {
        var filter = new FilterOperatorInfo("Name", FilterOperator.StartsWith, "Test");
        Assert.Equal("%Test", filter.ValuePattern());
    }

    [Fact]
    public void ValuePattern_EndsWithOperator_AddsTrailingWildcard()
    {
        var filter = new FilterOperatorInfo("Name", FilterOperator.EndsWith, "Test");
        Assert.Equal("Test%", filter.ValuePattern());
    }

    [Fact]
    public void ValuePattern_DoesNotContainOperator_AddsSurroundingWildcards()
    {
        var filter = new FilterOperatorInfo("Name", FilterOperator.DoesNotContain, "Test");
        Assert.Equal("%Test%", filter.ValuePattern());
    }

    [Fact]
    public void ValuePattern_IsNullOperator_ReturnsEmptyString()
    {
        var filter = new FilterOperatorInfo("Name", FilterOperator.IsNull, "ignored");
        Assert.Equal(string.Empty, filter.ValuePattern());
    }

    [Fact]
    public void ValuePattern_IsNotNullOperator_ReturnsEmptyString()
    {
        var filter = new FilterOperatorInfo("Name", FilterOperator.IsNotNull, "ignored");
        Assert.Equal(string.Empty, filter.ValuePattern());
    }

    [Fact]
    public void ValuePattern_EqualsOperator_ReturnsOriginalValue()
    {
        var filter = new FilterOperatorInfo("Name", FilterOperator.Equals, "Test");
        Assert.Equal("Test", filter.ValuePattern());
    }

    [Fact]
    public void ValuePattern_NumericValue_ReturnsUnmodified()
    {
        var filter = new FilterOperatorInfo("Age", FilterOperator.Contains, 42);
        Assert.Equal(42, filter.ValuePattern());
    }

    [Fact]
    public void AndJoinOperator_IsAnd()
    {
        Assert.Equal("AND", FilterOperatorInfo.AndJoinOperator);
    }

    [Fact]
    public void OrJoinOperator_IsOr()
    {
        Assert.Equal("OR", FilterOperatorInfo.OrJoinOperator);
    }
}
