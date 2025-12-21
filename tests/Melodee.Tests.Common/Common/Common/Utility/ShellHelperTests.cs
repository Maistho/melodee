using FluentAssertions;
using Melodee.Common.Utility;

namespace Melodee.Tests.Unit.Common.Utility;

public class ShellHelperTests
{
    [Fact]
    public async Task Bash_SimpleEchoCommand_ReturnsZeroExitCode()
    {
        var exitCode = await "echo 'Hello World'".Bash();

        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task Bash_CommandWithQuotes_EscapesCorrectly()
    {
        var exitCode = await "echo \"Test with quotes\"".Bash();

        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task Bash_InvalidCommand_ThrowsException()
    {
        var act = async () => await "nonexistentcommand12345".Bash();

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*failed with exit code*");
    }

    [Fact]
    public async Task Bash_CommandReturningNonZero_ThrowsException()
    {
        var act = async () => await "exit 1".Bash();

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*failed with exit code*");
    }

    [Fact]
    public async Task Bash_CommandWithStderr_CompletesSuccessfully()
    {
        var exitCode = await "echo 'error message' >&2".Bash();

        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task Bash_MultilineCommand_ExecutesSuccessfully()
    {
        var command = "if [ 1 -eq 1 ]; then echo 'true'; fi";

        var exitCode = await command.Bash();

        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task Bash_CommandWithPipe_ExecutesSuccessfully()
    {
        var exitCode = await "echo 'test' | cat".Bash();

        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task Bash_LongRunningCommand_CompletesSuccessfully()
    {
        var exitCode = await "sleep 0.1".Bash();

        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task Bash_CommandCreatingTempFile_CleansUpCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var command = $"echo 'test' > {tempFile}";
            var exitCode = await command.Bash();

            exitCode.Should().Be(0);
            File.Exists(tempFile).Should().BeTrue();
            var content = await File.ReadAllTextAsync(tempFile);
            content.Trim().Should().Be("test");
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task Bash_CommandWithVariables_EvaluatesCorrectly()
    {
        var exitCode = await "TEST_VAR='hello'; echo $TEST_VAR".Bash();

        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task Bash_FileTestCommand_WorksCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var command = $"test -f {tempFile}";
            var exitCode = await command.Bash();

            exitCode.Should().Be(0);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task Bash_FileTestNonExistent_ReturnsNonZero()
    {
        var command = "test -f /nonexistent/file/path/12345.txt";

        var act = async () => await command.Bash();

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Bash_CommandWithSpecialCharacters_HandlesCorrectly()
    {
        var exitCode = await "echo 'test@#$%'".Bash();

        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task Bash_EmptyCommand_HandlesGracefully()
    {
        var exitCode = await "true".Bash();

        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task Bash_WhichCommand_FindsBash()
    {
        var exitCode = await "which bash".Bash();

        exitCode.Should().Be(0);
    }
}
