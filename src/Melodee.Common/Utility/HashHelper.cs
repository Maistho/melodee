using System.Security.Cryptography;
using System.Text;
using K4os.Hash.xxHash;

namespace Melodee.Common.Utility;

public static class HashHelper
{
    // NOTE: MD5 methods are maintained for compatibility with external APIs (OpenSubsonic, Last.fm)
    // that require MD5 for authentication. For new code, use CreateSha256 instead.
    public static string? CreateMd5(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        return CreateMd5(Encoding.UTF8.GetBytes(input));
    }

    public static string? CreateMd5(FileInfo file)
    {
        // CodeQL [cs/weak-crypto] MD5 required by external APIs (OpenSubsonic, Last.fm) - use CreateSha256 for new code
        using var md5 = MD5.Create();
        using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
        var hash = md5.ComputeHash(stream);

        var sBuilder = new StringBuilder();
        foreach (var t in hash)
        {
            sBuilder.Append(t.ToString("x2"));
        }
        return sBuilder.ToString();
    }

    public static string? CreateMd5(byte[]? bytes)
    {
        if (bytes == null || !bytes.Any())
        {
            return null;
        }

        // CodeQL [cs/weak-crypto] MD5 required by external APIs (OpenSubsonic, Last.fm) - use CreateSha256 for new code
        using (var md5 = MD5.Create())
        {
            var data = md5.ComputeHash(bytes);

            // Create a new StringBuilder to collect the bytes and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data and format each one as a hexadecimal string.
            foreach (var t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }

    public static string? CreateSha256(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        return CreateSha256(Encoding.UTF8.GetBytes(input));
    }

    public static string? CreateSha256(FileInfo file)
    {
        using var sha256 = SHA256.Create();
        using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
        var hash = sha256.ComputeHash(stream);

        var sBuilder = new StringBuilder();
        foreach (var t in hash)
        {
            sBuilder.Append(t.ToString("x2"));
        }
        return sBuilder.ToString();
    }

    public static string? CreateSha256(byte[]? bytes)
    {
        if (bytes == null || !bytes.Any())
        {
            return null;
        }

        using (var sha256 = SHA256.Create())
        {
            var data = sha256.ComputeHash(bytes);

            // Create a new StringBuilder to collect the bytes and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data and format each one as a hexadecimal string.
            foreach (var t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }

    public static uint GetHash(string file)
    {
        return XXH32.DigestOf(File.ReadAllBytes(file));
    }
}
