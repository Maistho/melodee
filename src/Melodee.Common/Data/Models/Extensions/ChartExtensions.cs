using Melodee.Common.Data.Constants;

namespace Melodee.Common.Data.Models.Extensions;

public static class ChartExtensions
{
    public static string ToImageFileName(this Chart chart, string libraryPath)
    {
        return Path.Combine(libraryPath, Chart.ImagesDirectoryName, $"{chart.ApiKey}.gif");
    }

    public static string ToApiKey(this Chart chart)
    {
        return $"chart{OpenSubsonicServer.ApiIdSeparator}{chart.ApiKey}";
    }
}
