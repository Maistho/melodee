using Melodee.Common.Data.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Models;

namespace Melodee.Common.Data.Models.Extensions;

public static class LibraryExtensions
{
    public static string ToApiKey(this Library library)
    {
        return $"library{OpenSubsonicServer.ApiIdSeparator}{library.ApiKey}";
    }

    public static bool ShouldBeScanned(this Library library) =>
        library.TypeValue switch
        {
            LibraryType.Inbound or LibraryType.Staging or LibraryType.Storage => true,
            _ => false
        };

    public static void PurgePath(this Library library)
    {
        if (!Directory.Exists(library.Path))
        {
            return;
        }

        Directory.Delete(library.Path, true);
        if (!Directory.Exists(library.Path))
        {
            Directory.CreateDirectory(library.Path);
        }
    }

    public static FileSystemDirectoryInfo ToFileSystemDirectoryInfo(this Library library)
    {
        return new FileSystemDirectoryInfo
        {
            Path = library.Path,
            Name = library.Path
        };
    }
}
