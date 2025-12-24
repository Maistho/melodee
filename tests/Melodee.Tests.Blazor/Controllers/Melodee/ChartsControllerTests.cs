using System.Reflection;
using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for ChartsController to ensure it's read-only and properly configured.
/// </summary>
public class ChartsControllerTests
{
    #region Controller Attribute Tests

    [Fact]
    public void ChartsController_HasApiControllerAttribute()
    {
        var attribute = typeof(ChartsController).GetCustomAttributes(typeof(ApiControllerAttribute), false).FirstOrDefault();
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void ChartsController_HasCorrectRoutePrefix()
    {
        var routeAttribute = typeof(ChartsController).GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;

        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v{version:apiVersion}/charts");
    }

    #endregion

    #region Read-Only API Verification Tests

    [Fact]
    public void ChartsController_HasOnlyGetMethods()
    {
        var methods = typeof(ChartsController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        var httpMethods = methods
            .Where(m => m.GetCustomAttributes()
                .Any(a => a.GetType().Name.StartsWith("Http")))
            .ToList();

        httpMethods.Should().NotBeEmpty("Controller should have HTTP methods");

        foreach (var method in httpMethods)
        {
            var hasGet = method.GetCustomAttributes(typeof(HttpGetAttribute), false).Any();
            var hasPost = method.GetCustomAttributes(typeof(HttpPostAttribute), false).Any();
            var hasPut = method.GetCustomAttributes(typeof(HttpPutAttribute), false).Any();
            var hasDelete = method.GetCustomAttributes(typeof(HttpDeleteAttribute), false).Any();
            var hasPatch = method.GetCustomAttributes(typeof(HttpPatchAttribute), false).Any();

            hasGet.Should().BeTrue($"Method {method.Name} should only use HttpGet");
            hasPost.Should().BeFalse($"Method {method.Name} should NOT have HttpPost - charts API is read-only");
            hasPut.Should().BeFalse($"Method {method.Name} should NOT have HttpPut - charts API is read-only");
            hasDelete.Should().BeFalse($"Method {method.Name} should NOT have HttpDelete - charts API is read-only");
            hasPatch.Should().BeFalse($"Method {method.Name} should NOT have HttpPatch - charts API is read-only");
        }
    }

    [Fact]
    public void ChartsController_NoMutationEndpointsExist()
    {
        var controllerType = typeof(ChartsController);

        var postMethods = controllerType.GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(HttpPostAttribute), false).Any())
            .ToList();

        var putMethods = controllerType.GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(HttpPutAttribute), false).Any())
            .ToList();

        var deleteMethods = controllerType.GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(HttpDeleteAttribute), false).Any())
            .ToList();

        var patchMethods = controllerType.GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(HttpPatchAttribute), false).Any())
            .ToList();

        postMethods.Should().BeEmpty("ChartsController should not have POST methods - admin operations occur in Blazor UI");
        putMethods.Should().BeEmpty("ChartsController should not have PUT methods - admin operations occur in Blazor UI");
        deleteMethods.Should().BeEmpty("ChartsController should not have DELETE methods - admin operations occur in Blazor UI");
        patchMethods.Should().BeEmpty("ChartsController should not have PATCH methods - admin operations occur in Blazor UI");
    }

    #endregion

    #region ListAsync Endpoint Tests

    [Fact]
    public void ListAsync_HasHttpGetAttribute()
    {
        var method = typeof(ChartsController).GetMethod(nameof(ChartsController.ListAsync));

        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void ListAsync_HasCorrectParameters()
    {
        var method = typeof(ChartsController).GetMethod(nameof(ChartsController.ListAsync));

        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCountGreaterThan(0);

        var pageParam = parameters.FirstOrDefault(p => p.Name == "page");
        pageParam.Should().NotBeNull();

        var pageSizeParam = parameters.FirstOrDefault(p => p.Name == "pageSize");
        pageSizeParam.Should().NotBeNull();

        var tagsParam = parameters.FirstOrDefault(p => p.Name == "tags");
        tagsParam.Should().NotBeNull();

        var yearParam = parameters.FirstOrDefault(p => p.Name == "year");
        yearParam.Should().NotBeNull();

        var sourceParam = parameters.FirstOrDefault(p => p.Name == "source");
        sourceParam.Should().NotBeNull();
    }

    [Fact]
    public void ListAsync_ReturnsTaskOfIActionResult()
    {
        var method = typeof(ChartsController).GetMethod(nameof(ChartsController.ListAsync));

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<IActionResult>));
    }

    #endregion

    #region GetByIdOrSlug Endpoint Tests

    [Fact]
    public void GetByIdOrSlug_HasHttpGetAttribute()
    {
        var method = typeof(ChartsController).GetMethod(nameof(ChartsController.GetByIdOrSlug));

        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetByIdOrSlug_HasCorrectRouteAttribute()
    {
        var method = typeof(ChartsController).GetMethod(nameof(ChartsController.GetByIdOrSlug));

        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{idOrSlug}");
    }

    [Fact]
    public void GetByIdOrSlug_HasCorrectParameters()
    {
        var method = typeof(ChartsController).GetMethod(nameof(ChartsController.GetByIdOrSlug));

        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().Contain(p => p.Name == "idOrSlug");
        parameters.Should().Contain(p => p.Name == "cancellationToken");
    }

    #endregion

    #region GetPlaylistTracks Endpoint Tests

    [Fact]
    public void GetPlaylistTracks_HasHttpGetAttribute()
    {
        var method = typeof(ChartsController).GetMethod(nameof(ChartsController.GetPlaylistTracks));

        method.Should().NotBeNull();
        var httpGetAttribute = method!.GetCustomAttributes(typeof(HttpGetAttribute), false).FirstOrDefault();
        httpGetAttribute.Should().NotBeNull();
    }

    [Fact]
    public void GetPlaylistTracks_HasCorrectRouteAttribute()
    {
        var method = typeof(ChartsController).GetMethod(nameof(ChartsController.GetPlaylistTracks));

        method.Should().NotBeNull();
        var routeAttribute = method!.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("{idOrSlug}/playlist");
    }

    #endregion

    #region HTTP Method Count Verification

    [Fact]
    public void ChartsController_HasExactlyThreeEndpoints()
    {
        var methods = typeof(ChartsController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        var httpMethods = methods
            .Where(m => m.GetCustomAttributes()
                .Any(a => a.GetType().Name.StartsWith("Http")))
            .ToList();

        httpMethods.Should().HaveCount(3, "ChartsController should have exactly 3 read-only endpoints: ListAsync, GetByIdOrSlug, GetPlaylistTracks");
    }

    #endregion
}
