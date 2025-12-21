using Melodee.Common.Utility;

namespace Melodee.Tests.Common.Utility;

public class ShellHelperBashTests
{
    [Fact]
    public async Task Bash_SimpleEchoCommand_ReturnsZero()
    {
        var result = await "echo test".Bash();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Bash_TrueCommand_ReturnsZero()
    {
        var result = await "true".Bash();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Bash_FalseCommand_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(async () => await "false".Bash());
    }

    [Fact]
    public async Task Bash_NonZeroExitCode_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(async () => await "exit 1".Bash());
    }

    [Fact]
    public async Task Bash_ExitCode5_ThrowsExceptionWithCorrectCode()
    {
        var ex = await Assert.ThrowsAsync<Exception>(async () => await "exit 5".Bash());

        Assert.Contains("exit code `5`", ex.Message);
    }

    [Fact]
    public async Task Bash_CommandWithOutput_ReturnsZero()
    {
        var result = await "echo 'Hello World'".Bash();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Bash_MultipleCommands_ReturnsZero()
    {
        var result = await "echo test && echo test2".Bash();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Bash_CommandWithDoubleQuotes_EscapesCorrectly()
    {
        var result = await "echo \"quoted\"".Bash();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Bash_NonExistentCommand_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(async () => await "nonexistentcommandxyz123".Bash());
    }

    [Fact]
    public async Task Bash_PwdCommand_ReturnsZero()
    {
        var result = await "pwd".Bash();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Bash_DateCommand_ReturnsZero()
    {
        var result = await "date".Bash();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Bash_WcCommand_ReturnsZero()
    {
        var result = await "echo 'test' | wc -l".Bash();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Bash_TestOperator_Success_ReturnsZero()
    {
        var result = await "[ 1 -eq 1 ]".Bash();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Bash_TestOperator_Failure_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(async () => await "[ 1 -eq 2 ]".Bash());
    }

    [Fact]
    public async Task Bash_CommandWithVariables_ReturnsZero()
    {
        var result = await "VAR=test && echo $VAR".Bash();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Bash_CommandWithPipe_ReturnsZero()
    {
        var result = await "echo 'test' | grep test".Bash();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Bash_CommandWithRedirection_ReturnsZero()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var result = await $"echo 'test' > {tempFile}".Bash();

            Assert.Equal(0, result);
            Assert.True(File.Exists(tempFile));
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
    public async Task Bash_LongRunningCommand_ReturnsZero()
    {
        var result = await "sleep 0.1".Bash();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Bash_EmptyCommand_ReturnsZero()
    {
        var result = await "".Bash();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Bash_ExceptionMessage_ContainsCommand()
    {
        var ex = await Assert.ThrowsAsync<Exception>(async () => await "exit 42".Bash());

        Assert.Contains("exit 42", ex.Message);
    }
}
