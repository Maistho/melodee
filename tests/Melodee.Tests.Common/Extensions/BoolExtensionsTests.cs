using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class BoolExtensionsTests
{
    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void ToLowerCaseString_ReturnsLowercaseValue(bool input, string expected)
    {
        Assert.Equal(expected, input.ToLowerCaseString());
    }
}
