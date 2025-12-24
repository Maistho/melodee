using System.Reflection;

namespace Melodee.Blazor.Services;

public sealed class AppVersionProvider : IAppVersionProvider
{
    static readonly Lazy<string> CachedSemVerForDisplay = new(BuildSemVerForDisplay);

    public string GetSemVerForDisplay() => CachedSemVerForDisplay.Value;

    static string BuildSemVerForDisplay()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(informational))
        {
            var v = assembly.GetName().Version;
            return v is null ? string.Empty : $"{v.Major}.{v.Minor}.{Math.Max(0, v.Build)}";
        }

        var plusIndex = informational.IndexOf('+');
        return plusIndex >= 0 ? informational[..plusIndex] : informational;
    }
}
