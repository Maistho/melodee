using Melodee.Common.Enums;
using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class PictureIdentifierExtensionsTests
{
    [Theory]
    [InlineData(PictureIdentifier.Front, true)]
    [InlineData(PictureIdentifier.SecondaryFront, true)]
    [InlineData(PictureIdentifier.Artist, true)]
    [InlineData(PictureIdentifier.ArtistSecondary, true)]
    [InlineData(PictureIdentifier.NotSet, false)]
    [InlineData(PictureIdentifier.Back, false)]
    [InlineData(PictureIdentifier.Cd, false)]
    [InlineData(PictureIdentifier.Band, false)]
    [InlineData(PictureIdentifier.BandLogo, false)]
    [InlineData(PictureIdentifier.Leaflet, false)]
    [InlineData(PictureIdentifier.Generic, false)]
    [InlineData(PictureIdentifier.Unsupported, false)]
    public void ValidateIsSquare_ReturnsExpectedResult(PictureIdentifier identifier, bool expected)
    {
        Assert.Equal(expected, identifier.ValidateIsSquare());
    }
}
