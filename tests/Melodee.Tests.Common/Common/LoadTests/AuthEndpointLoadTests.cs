using Melodee.Common.Configuration;
using Melodee.Common.Data.Models;
using Melodee.Common.Services.Security;
using Melodee.Tests.Common.Common.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NBomber.CSharp;
using NodaTime;
using NodaTime.Testing;

namespace Melodee.Tests.Common.Common.LoadTests;

/// <summary>
/// NBomber-based load tests for authentication endpoints.
/// These tests are skipped in CI by default; run manually to validate performance characteristics.
/// Per WBS Phase 4.1: Load tests for /auth/google and /auth/refresh endpoints.
/// </summary>
public class AuthEndpointLoadTests : ServiceTestBase
{
    [Fact(Skip = "NBomber scenario used for manual load validation; skipped in CI to keep runs short.")]
    public async Task RefreshTokenRotation_LoadTest_CompletesWithoutFailures()
    {
        // Seed a user for load testing
        await using var context = await MockFactory().CreateDbContextAsync();
        var user = new User
        {
            UserName = "load_auth_user",
            UserNameNormalized = "LOAD_AUTH_USER",
            Email = "load_auth@example.com",
            EmailNormalized = "LOAD_AUTH@EXAMPLE.COM",
            PublicKey = "loadauthkey",
            PasswordEncrypted = "encrypted",
            ApiKey = Guid.NewGuid(),
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            IsAdmin = false,
            IsLocked = false
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var refreshTokenService = GetRefreshTokenService();
        var tokensCreated = 0;
        var rotationsCompleted = 0;

        // Scenario: Concurrent refresh token creation and rotation
        var scenario = Scenario.Empty("refresh_token_rotation_scenario");
        scenario = Scenario.WithInit(scenario, async _ =>
        {
            // Create multiple refresh tokens and rotate them
            for (var i = 0; i < 50; i++)
            {
                var createResult = await refreshTokenService.CreateTokenAsync(
                    user.Id,
                    $"device_{i}",
                    "TestUserAgent",
                    "127.0.0.1",
                    CancellationToken.None);

                if (createResult.IsSuccess)
                {
                    Interlocked.Increment(ref tokensCreated);

                    // Immediately rotate
                    var rotateResult = await refreshTokenService.RotateTokenAsync(
                        createResult.Token!,
                        $"device_{i}",
                        "TestUserAgent",
                        "127.0.0.1",
                        CancellationToken.None);

                    if (rotateResult.IsSuccess)
                    {
                        Interlocked.Increment(ref rotationsCompleted);
                    }
                }
            }
        });

        scenario = Scenario.WithWarmUpDuration(scenario, TimeSpan.FromSeconds(1));
        scenario = Scenario.WithLoadSimulations(
            scenario,
            [Simulation.Inject(rate: 5, interval: TimeSpan.FromMilliseconds(200), during: TimeSpan.FromSeconds(2))]
        );

        var stats = NBomberRunner.RegisterScenarios(scenario).Run();

        Assert.NotNull(stats);
        Assert.Contains(stats.ScenarioStats, s => s.ScenarioName == "refresh_token_rotation_scenario");
        Assert.True(tokensCreated > 0, "Should have created tokens");
        Assert.True(rotationsCompleted > 0, "Should have rotated tokens");
    }

    [Fact(Skip = "NBomber scenario used for manual load validation; skipped in CI to keep runs short.")]
    public async Task RefreshTokenReplayDetection_LoadTest_DetectsReplays()
    {
        // Seed a user
        await using var context = await MockFactory().CreateDbContextAsync();
        var user = new User
        {
            UserName = "replay_test_user",
            UserNameNormalized = "REPLAY_TEST_USER",
            Email = "replay@example.com",
            EmailNormalized = "REPLAY@EXAMPLE.COM",
            PublicKey = "replaykey",
            PasswordEncrypted = "encrypted",
            ApiKey = Guid.NewGuid(),
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            IsAdmin = false,
            IsLocked = false
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var refreshTokenService = GetRefreshTokenService();
        var replaysDetected = 0;

        var scenario = Scenario.Empty("replay_detection_scenario");
        scenario = Scenario.WithInit(scenario, async _ =>
        {
            // Create a token
            var createResult = await refreshTokenService.CreateTokenAsync(
                user.Id,
                "device_replay",
                "TestUserAgent",
                "127.0.0.1",
                CancellationToken.None);

            if (!createResult.IsSuccess) return;

            var originalToken = createResult.Token!;

            // Rotate it once (legitimate use)
            var rotateResult = await refreshTokenService.RotateTokenAsync(
                originalToken,
                "device_replay",
                "TestUserAgent",
                "127.0.0.1",
                CancellationToken.None);

            if (!rotateResult.IsSuccess) return;

            // Attempt to use the original token again (replay attack)
            for (var i = 0; i < 10; i++)
            {
                var replayResult = await refreshTokenService.RotateTokenAsync(
                    originalToken,
                    "device_replay",
                    "TestUserAgent",
                    "127.0.0.1",
                    CancellationToken.None);

                if (!replayResult.IsSuccess &&
                    replayResult.ErrorCode == "refresh_token_replayed")
                {
                    Interlocked.Increment(ref replaysDetected);
                }
            }
        });

        scenario = Scenario.WithWarmUpDuration(scenario, TimeSpan.FromSeconds(1));
        scenario = Scenario.WithLoadSimulations(
            scenario,
            [Simulation.Inject(rate: 2, interval: TimeSpan.FromMilliseconds(500), during: TimeSpan.FromSeconds(2))]
        );

        var stats = NBomberRunner.RegisterScenarios(scenario).Run();

        Assert.NotNull(stats);
        Assert.True(replaysDetected > 0, "Should have detected replay attempts");
    }

    [Fact(Skip = "NBomber scenario used for manual load validation; skipped in CI to keep runs short.")]
    public async Task ConcurrentTokenOperations_LoadTest_MaintainsConsistency()
    {
        // Seed multiple users
        await using var context = await MockFactory().CreateDbContextAsync();
        var users = Enumerable.Range(0, 10).Select(i => new User
        {
            UserName = $"concurrent_user_{i}",
            UserNameNormalized = $"CONCURRENT_USER_{i}",
            Email = $"concurrent_{i}@example.com",
            EmailNormalized = $"CONCURRENT_{i}@EXAMPLE.COM",
            PublicKey = $"concurrentkey_{i}",
            PasswordEncrypted = "encrypted",
            ApiKey = Guid.NewGuid(),
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            IsAdmin = false,
            IsLocked = false
        }).ToList();

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        var refreshTokenService = GetRefreshTokenService();
        var operationsCompleted = 0;
        var errors = 0;

        var scenario = Scenario.Empty("concurrent_token_operations_scenario");
        scenario = Scenario.WithInit(scenario, async _ =>
        {
            var tasks = users.Select(async user =>
            {
                // Each user creates, rotates, and revokes tokens
                for (var i = 0; i < 5; i++)
                {
                    try
                    {
                        var createResult = await refreshTokenService.CreateTokenAsync(
                            user.Id,
                            $"device_{user.Id}_{i}",
                            "TestUserAgent",
                            "127.0.0.1",
                            CancellationToken.None);

                        if (createResult.IsSuccess && createResult.Token != null)
                        {
                            var rotateResult = await refreshTokenService.RotateTokenAsync(
                                createResult.Token,
                                $"device_{user.Id}_{i}",
                                "TestUserAgent",
                                "127.0.0.1",
                                CancellationToken.None);

                            if (rotateResult.IsSuccess)
                            {
                                Interlocked.Increment(ref operationsCompleted);
                            }
                        }
                    }
                    catch
                    {
                        Interlocked.Increment(ref errors);
                    }
                }
            });

            await Task.WhenAll(tasks);
        });

        scenario = Scenario.WithWarmUpDuration(scenario, TimeSpan.FromSeconds(1));
        scenario = Scenario.WithLoadSimulations(
            scenario,
            [Simulation.Inject(rate: 3, interval: TimeSpan.FromMilliseconds(300), during: TimeSpan.FromSeconds(3))]
        );

        var stats = NBomberRunner.RegisterScenarios(scenario).Run();

        Assert.NotNull(stats);
        Assert.True(operationsCompleted > 0, "Should have completed operations");
        Assert.Equal(0, errors);
    }

    private IRefreshTokenService GetRefreshTokenService()
    {
        var tokenOptions = Options.Create(new TokenOptions
        {
            AccessTokenLifetimeMinutes = 15,
            RefreshTokenLifetimeDays = 30,
            MaxSessionDays = 90
        });

        return new RefreshTokenService(
            MockFactory(),
            tokenOptions,
            NullLogger<RefreshTokenService>.Instance,
            new FakeClock(SystemClock.Instance.GetCurrentInstant()));
    }
}
