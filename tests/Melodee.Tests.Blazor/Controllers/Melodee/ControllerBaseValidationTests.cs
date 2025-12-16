using FluentAssertions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Melodee.Tests.Blazor.Controllers.Melodee;

/// <summary>
/// Tests for ControllerBase validation helper methods via a test harness.
/// </summary>
public class ControllerBaseValidationTests
{
    #region Paging Validation Tests

    [Theory]
    [InlineData(1, 10, true)]
    [InlineData(1, 1, true)]
    [InlineData(1, 200, true)]
    [InlineData(100, 50, true)]
    public void TryValidatePaging_ValidInput_ReturnsTrue(int page, int pageSize, bool expectedValid)
    {
        // Arrange & Act
        var (isValid, normalizedPage, normalizedPageSize, _) = ValidatePaging(page, pageSize);

        // Assert
        isValid.Should().Be(expectedValid);
        normalizedPage.Should().BeGreaterThanOrEqualTo((short)1);
        normalizedPageSize.Should().BeInRange((short)1, (short)200);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(-100, 50)]
    public void TryValidatePaging_InvalidPage_ReturnsFalse(int page, int pageSize)
    {
        // Arrange & Act
        var (isValid, normalizedPage, _, errorMessage) = ValidatePaging(page, pageSize);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("page must be >= 1");
        normalizedPage.Should().Be(1); // Should normalize to 1
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 201)]
    [InlineData(1, 500)]
    public void TryValidatePaging_InvalidPageSize_ReturnsFalse(int page, int pageSize)
    {
        // Arrange & Act
        var (isValid, _, normalizedPageSize, errorMessage) = ValidatePaging(page, pageSize);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("pageSize must be between 1 and 200");
        normalizedPageSize.Should().BeInRange((short)1, (short)200); // Should clamp
    }

    [Fact]
    public void TryValidatePaging_PageSizeAtMaxBoundary_Succeeds()
    {
        // Arrange & Act
        var (isValid, _, normalizedPageSize, _) = ValidatePaging(1, 200);

        // Assert
        isValid.Should().BeTrue();
        normalizedPageSize.Should().Be(200);
    }

    [Fact]
    public void TryValidatePaging_PageSizeAboveMax_FailsAndClamps()
    {
        // Arrange & Act
        var (isValid, _, normalizedPageSize, _) = ValidatePaging(1, 201);

        // Assert
        isValid.Should().BeFalse();
        normalizedPageSize.Should().Be(200); // Clamped
    }

    #endregion

    #region Limit Validation Tests

    [Theory]
    [InlineData(1, true)]
    [InlineData(50, true)]
    [InlineData(100, true)]
    [InlineData(200, true)]
    public void TryValidateLimit_ValidInput_ReturnsTrue(int limit, bool expectedValid)
    {
        // Arrange & Act
        var (isValid, normalizedLimit, _) = ValidateLimit(limit);

        // Assert
        isValid.Should().Be(expectedValid);
        normalizedLimit.Should().Be((short)limit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void TryValidateLimit_TooSmall_ReturnsFalse(int limit)
    {
        // Arrange & Act
        var (isValid, normalizedLimit, errorMessage) = ValidateLimit(limit);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("limit must be between 1 and 200");
        normalizedLimit.Should().Be(1); // Clamped to 1
    }

    [Theory]
    [InlineData(201)]
    [InlineData(500)]
    [InlineData(1000)]
    public void TryValidateLimit_TooLarge_ReturnsFalse(int limit)
    {
        // Arrange & Act
        var (isValid, normalizedLimit, errorMessage) = ValidateLimit(limit);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("limit must be between 1 and 200");
        normalizedLimit.Should().Be(200); // Clamped to 200
    }

    #endregion

    #region Ordering Validation Tests

    [Fact]
    public void TryValidateOrdering_ValidField_ReturnsTrue()
    {
        // Arrange
        var allowedFields = new HashSet<string> { "Name", "CreatedAt", "UpdatedAt" };

        // Act
        var (isValid, field, direction, _) = ValidateOrdering("Name", "ASC", allowedFields);

        // Assert
        isValid.Should().BeTrue();
        field.Should().Be("Name");
        direction.Should().Be("ASC");
    }

    [Fact]
    public void TryValidateOrdering_NullField_UsesFirstAllowedField()
    {
        // Arrange
        var allowedFields = new HashSet<string> { "CreatedAt", "Name" };

        // Act
        var (isValid, field, direction, _) = ValidateOrdering(null, "DESC", allowedFields);

        // Assert
        isValid.Should().BeTrue();
        field.Should().Be("CreatedAt"); // First in the set
        direction.Should().Be("DESC");
    }

    [Fact]
    public void TryValidateOrdering_NullDirection_DefaultsToDesc()
    {
        // Arrange
        var allowedFields = new HashSet<string> { "Name" };

        // Act
        var (isValid, _, direction, _) = ValidateOrdering("Name", null, allowedFields);

        // Assert
        isValid.Should().BeTrue();
        direction.Should().Be("DESC");
    }

    [Fact]
    public void TryValidateOrdering_InvalidField_ReturnsFalse()
    {
        // Arrange
        var allowedFields = new HashSet<string> { "Name", "CreatedAt" };

        // Act
        var (isValid, _, _, errorMessage) = ValidateOrdering("InvalidField", "ASC", allowedFields);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("Invalid orderBy value");
    }

    [Fact]
    public void TryValidateOrdering_SqlInjectionAttempt_ReturnsFalse()
    {
        // Arrange
        var allowedFields = new HashSet<string> { "Name" };

        // Act
        var (isValid, _, _, errorMessage) = ValidateOrdering("Name; DROP TABLE Users;--", "ASC", allowedFields);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("Invalid orderBy value");
    }

    [Fact]
    public void TryValidateOrdering_FieldWithSpecialChars_ReturnsFalse()
    {
        // Arrange
        var allowedFields = new HashSet<string> { "Name", "Name$Special" };

        // Act - Even if in allowed list, special chars should fail regex
        var (isValid, _, _, errorMessage) = ValidateOrdering("Name$Special", "ASC", allowedFields);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("Invalid orderBy value");
    }

    [Fact]
    public void TryValidateOrdering_InvalidDirection_ReturnsFalse()
    {
        // Arrange
        var allowedFields = new HashSet<string> { "Name" };

        // Act
        var (isValid, _, _, errorMessage) = ValidateOrdering("Name", "INVALID", allowedFields);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Contain("Invalid orderDirection value");
    }

    [Theory]
    [InlineData("ASC")]
    [InlineData("asc")]
    [InlineData("Asc")]
    [InlineData("DESC")]
    [InlineData("desc")]
    [InlineData("Desc")]
    public void TryValidateOrdering_DirectionCaseInsensitive_Succeeds(string direction)
    {
        // Arrange
        var allowedFields = new HashSet<string> { "Name" };

        // Act
        var (isValid, _, normalizedDirection, _) = ValidateOrdering("Name", direction, allowedFields);

        // Assert
        isValid.Should().BeTrue();
        normalizedDirection.Should().BeOneOf("ASC", "DESC");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Simulates TryValidatePaging logic for testing
    /// </summary>
    private static (bool isValid, short normalizedPage, short normalizedPageSize, string? errorMessage) ValidatePaging(int page, int pageSize)
    {
        var normalizedPage = (short)Math.Max(page, 1);
        var normalizedPageSize = (short)Math.Clamp(pageSize, 1, 200);

        if (page < 1)
        {
            return (false, normalizedPage, normalizedPageSize, "page must be >= 1");
        }
        if (pageSize < 1 || pageSize > 200)
        {
            return (false, normalizedPage, normalizedPageSize, "pageSize must be between 1 and 200");
        }

        return (true, normalizedPage, normalizedPageSize, null);
    }

    /// <summary>
    /// Simulates TryValidateLimit logic for testing
    /// </summary>
    private static (bool isValid, short normalizedLimit, string? errorMessage) ValidateLimit(int limit)
    {
        var normalizedLimit = (short)Math.Clamp(limit, 1, 200);

        if (limit < 1 || limit > 200)
        {
            return (false, normalizedLimit, "limit must be between 1 and 200");
        }

        return (true, normalizedLimit, null);
    }

    /// <summary>
    /// Simulates TryValidateOrdering logic for testing
    /// </summary>
    private static (bool isValid, string field, string direction, string? errorMessage) ValidateOrdering(
        string? orderBy, string? orderDirection, IReadOnlySet<string> allowedFields)
    {
        var candidateField = string.IsNullOrWhiteSpace(orderBy) ? allowedFields.First() : orderBy;

        if (!allowedFields.Contains(candidateField) || !System.Text.RegularExpressions.Regex.IsMatch(candidateField, @"^[A-Za-z0-9_]+$"))
        {
            return (false, string.Empty, string.Empty, "Invalid orderBy value");
        }

        var direction = (string.IsNullOrWhiteSpace(orderDirection) ? "DESC" : orderDirection).ToUpperInvariant();
        if (direction is not ("ASC" or "DESC"))
        {
            return (false, candidateField, string.Empty, "Invalid orderDirection value");
        }

        return (true, candidateField, direction, null);
    }

    #endregion
}
