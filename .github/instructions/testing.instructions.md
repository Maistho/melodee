---
description: 'Testing conventions and best practices for xUnit test projects'
applyTo: '**/tests/**/*.cs, **/Melodee.Tests.*/**/*.cs'
---

# Testing Guidelines

## Test Project Structure

- Mirror the source project structure in test projects
- Name test classes `{ClassName}Tests`
- Group tests by the class or feature being tested
- Use separate files for different test scenarios when tests grow large

## Naming Conventions

- Use descriptive test method names that explain the scenario
- Follow pattern: `MethodName_StateUnderTest_ExpectedBehavior` or descriptive sentences
- Do not use "Test" prefix or suffix in method names

```csharp
// Good examples
public async Task GetAlbum_WithValidId_ReturnsAlbum()
public async Task ProcessTrack_WhenFileNotFound_ThrowsNotFoundException()
public async Task Search_EmptyQuery_ReturnsEmptyResults()
```

## Test Structure

- Use Arrange-Act-Assert pattern but do not add comments for each section
- Keep tests focused on single behavior
- Avoid logic in tests (no conditionals, loops)
- Extract common setup to helper methods or fixtures

```csharp
[Fact]
public async Task CreatePlaylist_WithValidData_ReturnsCreatedPlaylist()
{
    var service = CreatePlaylistService();
    var request = new CreatePlaylistRequest { Name = "Test Playlist" };

    var result = await service.CreateAsync(request);

    result.Should().NotBeNull();
    result.Name.Should().Be("Test Playlist");
}
```

## Assertions

- Use FluentAssertions for readable assertions
- Be specific with assertions; avoid generic `Assert.True`
- Test one logical concept per test method
- Include meaningful failure messages for complex assertions

```csharp
// Prefer FluentAssertions
result.Should().NotBeNull();
result.Tracks.Should().HaveCount(5);
result.Duration.Should().BeGreaterThan(TimeSpan.Zero);

// Avoid
Assert.True(result != null);
Assert.True(result.Tracks.Count == 5);
```

## Test Data

- Use descriptive variable names for test data
- Create factory methods or builders for complex objects
- Avoid magic numbers; use named constants
- Use realistic but not production data

## Mocking

- Use NSubstitute or Moq for mocking dependencies
- Mock at the boundary (repositories, external services)
- Don't mock the system under test
- Verify interactions only when behavior matters

```csharp
var mockRepository = Substitute.For<IAlbumRepository>();
mockRepository.GetByIdAsync(Arg.Any<int>())
    .Returns(new Album { Id = 1, Name = "Test Album" });
```

## Async Testing

- Use `async Task` for async test methods, not `async void`
- Await all async operations
- Use proper cancellation token handling in tests

## Test Categories

- Use `[Trait]` to categorize tests (Unit, Integration, Performance)
- Mark slow tests appropriately
- Use `[Skip]` with reason for temporarily disabled tests

```csharp
[Trait("Category", "Integration")]
public class DatabaseIntegrationTests { }

[Fact(Skip = "Requires external service - run manually")]
public async Task ExternalApiTest() { }
```

## Integration Tests

- Use `WebApplicationFactory` for API integration tests
- Use test containers or in-memory databases for data tests
- Clean up test data after each test
- Isolate tests to prevent interference

## Code Coverage

- Focus on testing critical business logic
- Don't chase coverage numbers at expense of test quality
- Use coverage reports to identify untested paths
- Exclude generated code and trivial properties from coverage
