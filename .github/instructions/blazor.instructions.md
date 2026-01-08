---
description: 'Blazor component and application patterns'
applyTo: '**/*.razor, **/*.razor.cs, **/*.razor.css'
---

## Blazor Code Style and Structure

- Write idiomatic and efficient Blazor and C# code.
- Follow .NET and Blazor conventions.
- Use Razor Components appropriately for component-based UI development.
- Prefer inline functions for smaller components but separate complex logic into code-behind or service classes.
- Async/await should be used where applicable to ensure non-blocking UI operations.

## Naming Conventions

- Follow PascalCase for component names, method names, and public members.
- Use camelCase for private fields and local variables.
- Prefix interface names with "I" (e.g., IUserService).

## Blazor and .NET Specific Guidelines

- Utilize Blazor's built-in features for component lifecycle (e.g., OnInitializedAsync, OnParametersSetAsync).
- Use data binding effectively with @bind.
- Leverage Dependency Injection for services in Blazor.
- Structure Blazor components and services following Separation of Concerns.
- Always use the latest version C#, currently C# 13 features (targeting .NET 10) like record types, pattern matching, and global usings.

## CRITICAL: Blazor Components MUST Use Injected Services Directly

**Melodee.Blazor is a Blazor Server application. Razor components should NEVER make HTTP API calls to the backend.**

### Why This Matters
- Blazor Server runs on the same server as the ASP.NET Core backend
- HTTP calls add unnecessary network overhead, serialization/deserialization, and latency
- Direct service injection provides better performance and type safety
- Follows proper Separation of Concerns - components use services, not HTTP clients

### Forbidden Patterns
```razor
@* WRONG - Do NOT do this *@
@inject IHttpClientFactory HttpClientFactory
var httpClient = HttpClientFactory.CreateClient("MelodeeApi");
var response = await httpClient.PostAsJsonAsync("api/v1/artist-lookup", request);

@* WRONG - Do NOT do this *@
var response = await HttpClient.GetFromJsonAsync<SomeResult>("/api/some-endpoint");
```

### Required Patterns
```razor
@* CORRECT - Inject and use services directly *@
@inject ArtistSearchEngineService ArtistSearchEngineService
@inject IMelodeeConfigurationFactory ConfigurationFactory

var result = await ArtistSearchEngineService.LookupAsync(artistName, limit, providerIds, cancellationToken);
var config = await ConfigurationFactory.GetConfigurationAsync();
```

### Service Injection Guidelines
1. **Inject service classes directly** - not HttpClient or IHttpClientFactory
2. **Use existing services** - Melodee.Blazor has services like ArtistService, AlbumService, ArtistSearchEngineService, etc.
3. **Controller classes are for external API clients** - not for Blazor components to call
4. **If a needed service doesn't exist**, create one following the existing patterns in Melodee.Common.Services
5. **Always await async service methods** - Blazor components should use async/await properly

### Examples of Correct Service Usage
- Use `ArtistService` instead of calling `/api/v1/artists` endpoints
- Use `ArtistSearchEngineService` instead of calling `/api/v1/artist-lookup` endpoints
- Use `AlbumService` instead of calling `/api/v1/albums` endpoints
- Use `ConfigurationFactory` instead of calling configuration endpoints

### When HTTP is Acceptable
HTTP calls are ONLY acceptable for:
1. External third-party APIs (Spotify, MusicBrainz, etc.)
2. If explicitly required for cross-application communication (rare)
3. For Blazor WebAssembly (this is a Blazor Server app, not Blazor WASM)

## Error Handling and Validation

- Implement proper error handling for Blazor pages and service calls (not HTTP API calls).
- Use logging for error tracking in the backend and consider capturing UI-level errors in Blazor with tools like ErrorBoundary.
- Implement validation using FluentValidation or DataAnnotations in forms.
- Wrap service calls in try-catch blocks and notify users appropriately.

## Blazor API and Performance Optimization

- Utilize Blazor server-side or WebAssembly optimally based on the project requirements.
- Use asynchronous methods (async/await) for service calls or UI actions that could block the main thread.
- Optimize Razor components by reducing unnecessary renders and using StateHasChanged() efficiently.
- Minimize the component render tree by avoiding re-renders unless necessary, using ShouldRender() where appropriate.
- Use EventCallbacks for handling user interactions efficiently, passing only minimal data when triggering events.
- **NEVER** make HTTP calls to backend APIs from Razor components - inject and use services directly for optimal performance.

## Caching Strategies

- Implement in-memory caching for frequently used data, especially for Blazor Server apps. Use IMemoryCache for lightweight caching solutions.
- For Blazor WebAssembly, utilize localStorage or sessionStorage to cache application state between user sessions.
- Consider Distributed Cache strategies (like Redis or SQL Server Cache) for larger applications that need shared state across multiple users or clients.
- Cache service call results where appropriate to avoid redundant operations, thus improving the user experience.
- Services in Melodee.Common already implement caching where appropriate - leverage those services rather than adding additional caching layers.

## State Management Libraries

- Use Blazor's built-in Cascading Parameters and EventCallbacks for basic state sharing across components.
- Implement advanced state management solutions using libraries like Fluxor or BlazorState when the application grows in complexity.
- For client-side state persistence in Blazor WebAssembly, consider using Blazored.LocalStorage or Blazored.SessionStorage to maintain state between page reloads.
- For server-side Blazor, use Scoped Services and the StateContainer pattern to manage state within user sessions while minimizing re-renders.

## API Design and Integration (For External APIs Only)

- Use HttpClient only for communicating with **external third-party APIs** (Spotify, MusicBrainz, etc.)
- Blazor components should **NOT** call internal backend APIs - inject and use services directly instead
- Implement error handling for external API calls using try-catch and provide proper user feedback in the UI.
- For external APIs, use IHttpClientFactory with named clients to properly configure each service

## Testing and Debugging in Visual Studio

- All unit testing and integration testing should be done in Visual Studio Enterprise.
- Test Blazor components and services using xUnit, NUnit, or MSTest.
- Use Moq or NSubstitute for mocking dependencies during tests.
- Debug Blazor UI issues using browser developer tools and Visual Studio's debugging tools for backend and server-side issues.
- For performance profiling and optimization, rely on Visual Studio's diagnostics tools.

## Security and Authentication

- Implement Authentication and Authorization in the Blazor app where necessary using ASP.NET Identity or JWT tokens for API authentication.
- Use HTTPS for all web communication and ensure proper CORS policies are implemented.

## API Documentation and Swagger

- Use Swagger/OpenAPI for API documentation for your backend API services.
- Ensure XML documentation for models and API methods for enhancing Swagger documentation.
