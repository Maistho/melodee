using System.Diagnostics;

namespace Melodee.Common.Utility;

// WARNING: This class executes shell commands and may pose a security risk if used with untrusted input.
// Only use with paths and commands from trusted configuration sources.
// The current escape mechanism only handles double quotes and does NOT protect against all shell injection vectors
// such as backticks, semicolons, pipe operators, or other shell metacharacters.
// Consider validating that script paths are within expected directories and using allowlists for acceptable scripts.
public static class ShellHelper
{
    public static Task<int> Bash(this string cmd)
    {
        var source = new TaskCompletionSource<int>();
        // WARNING: This escape only handles double quotes. Not comprehensive protection against shell injection.
        // This should only be used with trusted input from configuration files.
        var escapedArgs = cmd.Replace("\"", "\\\"");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };
        process.Exited += (sender, args) =>
        {
            Trace.WriteLine(process.StandardError.ReadToEnd(), "Warning");
            Trace.WriteLine(process.StandardOutput.ReadToEnd(), "Information");
            if (process.ExitCode == 0)
            {
                source.SetResult(0);
            }
            else
            {
                source.SetException(new Exception($"Command `{cmd}` failed with exit code `{process.ExitCode}`"));
            }

            process.Dispose();
        };

        try
        {
            process.Start();
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Command Line [{cmd}] Failed Error [{e}", "Error");
            source.SetException(e);
        }

        return source.Task;
    }
}
