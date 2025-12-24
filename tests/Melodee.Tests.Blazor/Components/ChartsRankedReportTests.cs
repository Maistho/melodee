namespace Melodee.Tests.Blazor.Components;

public class ChartsRankedReportTests
{
    [Fact]
    public void RankedAlbumInfo_SingleChartRankOne_CalculatesCorrectScore()
    {
        // Arrange
        var albumInfo = new RankedAlbumInfo
        {
            ArtistName = "Test Artist",
            AlbumTitle = "Test Album",
            Rankings =
            [
                new ChartRanking { Rank = 1 }
            ]
        };

        // Act
        albumInfo.CalculateStatistics();

        // Assert
        Assert.Equal(1, albumInfo.ChartCount);
        Assert.Equal(1.0, albumInfo.AverageRank);
        Assert.Equal(1, albumInfo.BestRank);
        Assert.Equal(1, albumInfo.WorstRank);
        // Score = 1.0 - (1 * 2) - 2.5 = -3.5
        Assert.Equal(-3.5, albumInfo.CompositeScore);
    }

    [Fact]
    public void RankedAlbumInfo_TwoChartsRankOneAndThree_CalculatesCorrectScore()
    {
        // Arrange
        var albumInfo = new RankedAlbumInfo
        {
            ArtistName = "Frank Sinatra",
            AlbumTitle = "In the Wee Small Hours",
            Rankings =
            [
                new ChartRanking { Rank = 1 },
                new ChartRanking { Rank = 3 }
            ]
        };

        // Act
        albumInfo.CalculateStatistics();

        // Assert
        Assert.Equal(2, albumInfo.ChartCount);
        Assert.Equal(2.0, albumInfo.AverageRank);
        Assert.Equal(1, albumInfo.BestRank);
        Assert.Equal(3, albumInfo.WorstRank);
        // Score = 2.0 - (2 * 2) - 2.5 = -4.5
        Assert.Equal(-4.5, albumInfo.CompositeScore);
    }

    [Fact]
    public void RankedAlbumInfo_MultiChartAlbum_RanksBetterThanSingleChart()
    {
        // Arrange
        var singleChartAlbum = new RankedAlbumInfo
        {
            ArtistName = "Single Chart Artist",
            AlbumTitle = "Single Chart Album",
            Rankings = [new ChartRanking { Rank = 1 }]
        };

        var multiChartAlbum = new RankedAlbumInfo
        {
            ArtistName = "Multi Chart Artist",
            AlbumTitle = "Multi Chart Album",
            Rankings =
            [
                new ChartRanking { Rank = 1 },
                new ChartRanking { Rank = 3 }
            ]
        };

        // Act
        singleChartAlbum.CalculateStatistics();
        multiChartAlbum.CalculateStatistics();

        // Assert - Multi-chart should have better (more negative) score
        Assert.True(multiChartAlbum.CompositeScore < singleChartAlbum.CompositeScore,
            $"Multi-chart score {multiChartAlbum.CompositeScore} should be better than single-chart score {singleChartAlbum.CompositeScore}");
    }

    [Fact]
    public void RankedAlbumInfo_FiveChartsAllRankOne_CalculatesCorrectScore()
    {
        // Arrange
        var albumInfo = new RankedAlbumInfo
        {
            ArtistName = "Legendary Artist",
            AlbumTitle = "Classic Album",
            Rankings =
            [
                new ChartRanking { Rank = 1 },
                new ChartRanking { Rank = 1 },
                new ChartRanking { Rank = 1 },
                new ChartRanking { Rank = 1 },
                new ChartRanking { Rank = 1 }
            ]
        };

        // Act
        albumInfo.CalculateStatistics();

        // Assert
        Assert.Equal(5, albumInfo.ChartCount);
        Assert.Equal(1.0, albumInfo.AverageRank);
        Assert.Equal(1, albumInfo.BestRank);
        Assert.Equal(1, albumInfo.WorstRank);
        // Score = 1.0 - (5 * 2) - 2.5 = -11.5
        Assert.Equal(-11.5, albumInfo.CompositeScore);
    }

    [Fact]
    public void RankedAlbumInfo_VariedRanksOnFiveCharts_CalculatesCorrectScore()
    {
        // Arrange
        var albumInfo = new RankedAlbumInfo
        {
            ArtistName = "Varied Ranks Artist",
            AlbumTitle = "Varied Ranks Album",
            Rankings =
            [
                new ChartRanking { Rank = 1 },
                new ChartRanking { Rank = 2 },
                new ChartRanking { Rank = 3 },
                new ChartRanking { Rank = 4 },
                new ChartRanking { Rank = 5 }
            ]
        };

        // Act
        albumInfo.CalculateStatistics();

        // Assert
        Assert.Equal(5, albumInfo.ChartCount);
        Assert.Equal(3.0, albumInfo.AverageRank); // (1+2+3+4+5)/5 = 3
        Assert.Equal(1, albumInfo.BestRank);
        Assert.Equal(5, albumInfo.WorstRank);
        // Score = 3.0 - (5 * 2) - 2.5 = -9.5
        Assert.Equal(-9.5, albumInfo.CompositeScore);
    }

    [Fact]
    public void RankedAlbumInfo_ConsistentTopRank_RanksBetterThanVariedRanks()
    {
        // Arrange
        var consistentAlbum = new RankedAlbumInfo
        {
            Rankings =
            [
                new ChartRanking { Rank = 1 },
                new ChartRanking { Rank = 1 },
                new ChartRanking { Rank = 1 },
                new ChartRanking { Rank = 1 },
                new ChartRanking { Rank = 1 }
            ]
        };

        var variedAlbum = new RankedAlbumInfo
        {
            Rankings =
            [
                new ChartRanking { Rank = 1 },
                new ChartRanking { Rank = 2 },
                new ChartRanking { Rank = 3 },
                new ChartRanking { Rank = 4 },
                new ChartRanking { Rank = 5 }
            ]
        };

        // Act
        consistentAlbum.CalculateStatistics();
        variedAlbum.CalculateStatistics();

        // Assert - Consistent #1 should rank better
        Assert.True(consistentAlbum.CompositeScore < variedAlbum.CompositeScore,
            $"Consistent rank-1 score {consistentAlbum.CompositeScore} should be better than varied ranks score {variedAlbum.CompositeScore}");
    }

    [Fact]
    public void RankedAlbumInfo_LowerRanksMultipleCharts_CalculatesPositiveScore()
    {
        // Arrange
        var albumInfo = new RankedAlbumInfo
        {
            ArtistName = "Average Artist",
            AlbumTitle = "Average Album",
            Rankings =
            [
                new ChartRanking { Rank = 15 },
                new ChartRanking { Rank = 20 },
                new ChartRanking { Rank = 25 }
            ]
        };

        // Act
        albumInfo.CalculateStatistics();

        // Assert
        Assert.Equal(3, albumInfo.ChartCount);
        Assert.Equal(20.0, albumInfo.AverageRank);
        Assert.Equal(15, albumInfo.BestRank);
        Assert.Equal(25, albumInfo.WorstRank);
        // Score = 20.0 - (3 * 2) - 0 = 14.0 (no best rank bonus for rank 15)
        Assert.Equal(14.0, albumInfo.CompositeScore);
    }

    [Fact]
    public void RankedAlbumInfo_BestRankBonus_AppliesOnlyForTopFive()
    {
        // Arrange
        var rank1Album = new RankedAlbumInfo
        {
            Rankings = [new ChartRanking { Rank = 1 }]
        };

        var rank5Album = new RankedAlbumInfo
        {
            Rankings = [new ChartRanking { Rank = 5 }]
        };

        var rank6Album = new RankedAlbumInfo
        {
            Rankings = [new ChartRanking { Rank = 6 }]
        };

        // Act
        rank1Album.CalculateStatistics();
        rank5Album.CalculateStatistics();
        rank6Album.CalculateStatistics();

        // Assert
        // Rank 1: 1.0 - 2.0 - 2.5 = -3.5
        Assert.Equal(-3.5, rank1Album.CompositeScore);
        // Rank 5: 5.0 - 2.0 - 0.5 = 2.5
        Assert.Equal(2.5, rank5Album.CompositeScore);
        // Rank 6: 6.0 - 2.0 - 0 = 4.0 (no bonus)
        Assert.Equal(4.0, rank6Album.CompositeScore);
    }

    [Fact]
    public void RankedAlbumInfo_SortingByCompositeScore_OrdersCorrectly()
    {
        // Arrange
        var albums = new List<RankedAlbumInfo>
        {
            new() { ArtistName = "C", Rankings = [new ChartRanking { Rank = 1 }] },
            new() { ArtistName = "A", Rankings = [new ChartRanking { Rank = 1 }, new ChartRanking { Rank = 3 }] },
            new() { ArtistName = "B", Rankings = [new ChartRanking { Rank = 15 }] }
        };

        // Act
        foreach (var album in albums)
        {
            album.CalculateStatistics();
        }

        var sorted = albums.OrderBy(a => a.CompositeScore).ToList();

        // Assert - Best to worst: A (multi-chart), C (single #1), B (single #15)
        Assert.Equal("A", sorted[0].ArtistName); // -4.5
        Assert.Equal("C", sorted[1].ArtistName); // -3.5
        Assert.Equal("B", sorted[2].ArtistName); // 13.0
    }

    // Helper classes matching the ChartsRankedReport.razor structure
    private sealed class RankedAlbumInfo
    {
        public string ArtistName { get; set; } = string.Empty;
        public string AlbumTitle { get; set; } = string.Empty;
        public List<ChartRanking> Rankings { get; set; } = [];

        public int ChartCount => Rankings.Count;
        public double AverageRank { get; private set; }
        public int BestRank { get; private set; }
        public int WorstRank { get; private set; }
        public double CompositeScore { get; private set; }

        public void CalculateStatistics()
        {
            if (Rankings.Count == 0) return;

            AverageRank = Rankings.Average(r => r.Rank);
            BestRank = Rankings.Min(r => r.Rank);
            WorstRank = Rankings.Max(r => r.Rank);

            var chartCountBonus = ChartCount * 2.0;
            var bestRankBonus = BestRank <= 5 ? (6 - BestRank) * 0.5 : 0;

            CompositeScore = AverageRank - chartCountBonus - bestRankBonus;
        }
    }

    private sealed class ChartRanking
    {
        public int Rank { get; set; }
    }
}
