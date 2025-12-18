using Melodee.Common.Enums;
using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class ContributorTypeExtensionsTests
{
    [Theory]
    [InlineData(ContributorType.Publisher, true)]
    [InlineData(ContributorType.NotSet, false)]
    [InlineData(ContributorType.Performer, false)]
    [InlineData(ContributorType.Production, false)]
    public void RestrictToOnePerAlbum_ReturnsExpectedResult(ContributorType type, bool expected)
    {
        Assert.Equal(expected, type.RestrictToOnePerAlbum());
    }
}
