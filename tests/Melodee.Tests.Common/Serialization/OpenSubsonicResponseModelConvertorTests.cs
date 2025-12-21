using System.Text.Json;
using FluentAssertions;
using Melodee.Common.Models;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Serialization;
using Melodee.Common.Serialization.Convertors;
using Xunit;

namespace Melodee.Tests.Common.Serialization;

public class OpenSubsonicResponseModelConvertorTests
{
    private readonly JsonSerializerOptions _options;

    public OpenSubsonicResponseModelConvertorTests()
    {
        _options = new JsonSerializerOptions(Serializer.JsonSerializerOptions);
        _options.Converters.Add(new OpenSubsonicResponseModelConvertor());
    }

    private string LoadFixture(string filename)
    {
        var baseDir = AppContext.BaseDirectory;
        var testProjectDir = System.IO.Directory.GetParent(baseDir)!.Parent!.Parent!.Parent!.FullName;
        var path = Path.Combine(testProjectDir, "Serialization", "Fixtures", "OpenSubsonic", filename);
        return File.ReadAllText(path);
    }

    [Fact]
    public void Read_SuccessPingResponse_ParsesCorrectly()
    {
        var json = LoadFixture("success_ping.json");

        var result = JsonSerializer.Deserialize<ResponseModel>(json, _options);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.ResponseData.Should().NotBeNull();
        result.ResponseData.IsSuccess.Should().BeTrue();
        result.ResponseData.Version.Should().Be("1.16.1");
        result.ResponseData.Type.Should().Be("melodee");
        result.ResponseData.ServerVersion.Should().Be("1.0.0");
        result.ResponseData.Error.Should().BeNull();
    }

    [Fact]
    public void Read_SuccessResponseWithData_ParsesDataCorrectly()
    {
        var json = LoadFixture("success_with_data.json");

        var result = JsonSerializer.Deserialize<ResponseModel>(json, _options);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.ResponseData.DataPropertyName.Should().Be("albumList");
        result.ResponseData.DataDetailPropertyName.Should().Be("album");
        result.ResponseData.Data.Should().NotBeNull();
    }

    [Fact]
    public void Read_ErrorResponse_ParsesErrorCorrectly()
    {
        var json = LoadFixture("error_response.json");

        var result = JsonSerializer.Deserialize<ResponseModel>(json, _options);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeFalse();
        result.ResponseData.Error.Should().NotBeNull();
        result.ResponseData.Error!.Code.Should().Be(40);
        result.ResponseData.Error.Message.Should().Be("Wrong username or password.");
    }

    [Fact]
    public void Read_FlatPayloadWithoutSubsonicResponseWrapper_ParsesCorrectly()
    {
        var json = LoadFixture("flat_payload.json");

        var result = JsonSerializer.Deserialize<ResponseModel>(json, _options);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.ResponseData.Version.Should().Be("1.16.1");
        result.ResponseData.Type.Should().Be("melodee");
        result.ResponseData.DataPropertyName.Should().Be("ping");
    }

    [Fact]
    public void Read_MinimalRequiredFields_UsesDefaults()
    {
        var json = LoadFixture("minimal_required.json");

        var result = JsonSerializer.Deserialize<ResponseModel>(json, _options);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.ResponseData.Version.Should().BeEmpty();
        result.ResponseData.Type.Should().BeEmpty();
        result.ResponseData.ServerVersion.Should().BeEmpty();
    }

    [Fact]
    public void Read_MissingStatus_DefaultsToFalse()
    {
        var json = LoadFixture("missing_status.json");

        var result = JsonSerializer.Deserialize<ResponseModel>(json, _options);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeFalse();
        result.ResponseData.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData("invalid json")]
    [InlineData("")]
    public void Read_InvalidJson_ThrowsJsonException(string invalidJson)
    {
        var act = () => JsonSerializer.Deserialize<ResponseModel>(invalidJson, _options);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_NullJson_ReturnsNull()
    {
        var result = JsonSerializer.Deserialize<ResponseModel>("null", _options);

        result.Should().BeNull();
    }

    [Fact]
    public void Read_StatusOkCaseInsensitive_RecognizesSuccess()
    {
        var json = """
        {
          "subsonic-response": {
            "status": "OK",
            "version": "1.16.1"
          }
        }
        """;

        var result = JsonSerializer.Deserialize<ResponseModel>(json, _options);

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Read_ErrorWithMissingCode_DefaultsToZero()
    {
        var json = """
        {
          "subsonic-response": {
            "status": "failed",
            "error": {
              "message": "Test error"
            }
          }
        }
        """;

        var result = JsonSerializer.Deserialize<ResponseModel>(json, _options);

        result!.ResponseData.Error.Should().NotBeNull();
        result.ResponseData.Error!.Code.Should().Be(0);
        result.ResponseData.Error.Message.Should().Be("Test error");
    }

    [Fact]
    public void Read_ErrorWithMissingMessage_DefaultsToEmpty()
    {
        var json = """
        {
          "subsonic-response": {
            "status": "failed",
            "error": {
              "code": 50
            }
          }
        }
        """;

        var result = JsonSerializer.Deserialize<ResponseModel>(json, _options);

        result!.ResponseData.Error.Should().NotBeNull();
        result.ResponseData.Error!.Code.Should().Be(50);
        result.ResponseData.Error.Message.Should().BeEmpty();
    }

    [Fact]
    public void Read_NestedDataObject_ExtractsDetailPropertyName()
    {
        var json = """
        {
          "subsonic-response": {
            "status": "ok",
            "version": "1.16.1",
            "playlist": {
              "entry": [
                { "id": "1", "title": "Song 1" }
              ]
            }
          }
        }
        """;

        var result = JsonSerializer.Deserialize<ResponseModel>(json, _options);

        result!.ResponseData.DataPropertyName.Should().Be("playlist");
        result.ResponseData.DataDetailPropertyName.Should().Be("entry");
    }

    [Fact]
    public void Write_BasicSuccessResponse_CreatesCorrectJson()
    {
        var responseModel = new ResponseModel
        {
            IsSuccess = true,
            UserInfo = UserInfo.BlankUserInfo,
            ResponseData = new ApiResponse
            {
                IsSuccess = true,
                Version = "1.16.1",
                Type = "melodee",
                ServerVersion = "1.0.0",
                DataPropertyName = string.Empty
            },
            TotalCount = 0
        };

        var json = JsonSerializer.Serialize(responseModel, _options);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("subsonic-response", out var root).Should().BeTrue();
        root.GetProperty("status").GetString().Should().Be("ok");
        root.GetProperty("version").GetString().Should().Be("1.16.1");
        root.GetProperty("type").GetString().Should().Be("melodee");
        root.GetProperty("serverVersion").GetString().Should().Be("1.0.0");
        root.GetProperty("openSubsonic").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void Write_ErrorResponse_IncludesErrorObject()
    {
        var responseModel = new ResponseModel
        {
            IsSuccess = false,
            UserInfo = UserInfo.BlankUserInfo,
            ResponseData = new ApiResponse
            {
                IsSuccess = false,
                Version = "1.16.1",
                Type = "melodee",
                ServerVersion = "1.0.0",
                DataPropertyName = string.Empty,
                Error = Error.AuthError
            },
            TotalCount = 0
        };

        var json = JsonSerializer.Serialize(responseModel, _options);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("subsonic-response", out var root).Should().BeTrue();
        root.GetProperty("status").GetString().Should().Be("error");
        root.TryGetProperty("error", out var error).Should().BeTrue();
        error.GetProperty("code").GetInt16().Should().Be(40);
        error.GetProperty("message").GetString().Should().Be("Wrong username or password.");
    }

    [Fact]
    public void Write_ResponseWithData_IncludesDataProperty()
    {
        var testData = new { id = "123", name = "Test" };
        var responseModel = new ResponseModel
        {
            IsSuccess = true,
            UserInfo = UserInfo.BlankUserInfo,
            ResponseData = new ApiResponse
            {
                IsSuccess = true,
                Version = "1.16.1",
                Type = "melodee",
                ServerVersion = "1.0.0",
                DataPropertyName = "ping",
                Data = testData
            },
            TotalCount = 0
        };

        var json = JsonSerializer.Serialize(responseModel, _options);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("subsonic-response", out var root).Should().BeTrue();
        root.TryGetProperty("ping", out var data).Should().BeTrue();
        data.GetProperty("id").GetString().Should().Be("123");
        data.GetProperty("name").GetString().Should().Be("Test");
    }

    [Fact]
    public void Write_ResponseWithDataDetailProperty_CreatesNestedStructure()
    {
        var testData = new[] { new { id = "1" }, new { id = "2" } };
        var responseModel = new ResponseModel
        {
            IsSuccess = true,
            UserInfo = UserInfo.BlankUserInfo,
            ResponseData = new ApiResponse
            {
                IsSuccess = true,
                Version = "1.16.1",
                Type = "melodee",
                ServerVersion = "1.0.0",
                DataPropertyName = "albumList",
                DataDetailPropertyName = "album",
                Data = testData
            },
            TotalCount = 0
        };

        var json = JsonSerializer.Serialize(responseModel, _options);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("subsonic-response", out var root).Should().BeTrue();
        root.TryGetProperty("albumList", out var albumList).Should().BeTrue();
        albumList.TryGetProperty("album", out var albums).Should().BeTrue();
        albums.GetArrayLength().Should().Be(2);
    }

    [Fact]
    public void Write_ResponseWithNullData_WritesPropertyWithoutValue()
    {
        var responseModel = new ResponseModel
        {
            IsSuccess = true,
            UserInfo = UserInfo.BlankUserInfo,
            ResponseData = new ApiResponse
            {
                IsSuccess = true,
                Version = "1.16.1",
                Type = "melodee",
                ServerVersion = "1.0.0",
                DataPropertyName = "test",
                Data = null
            },
            TotalCount = 0
        };

        var json = JsonSerializer.Serialize(responseModel, _options);

        json.Should().Contain("subsonic-response");
        json.Should().Contain("status");
    }

    [Fact]
    public void RoundTrip_SuccessResponse_MaintainsData()
    {
        var original = new ResponseModel
        {
            IsSuccess = true,
            UserInfo = UserInfo.BlankUserInfo,
            ResponseData = new ApiResponse
            {
                IsSuccess = true,
                Version = "1.16.1",
                Type = "melodee",
                ServerVersion = "1.0.0",
                DataPropertyName = string.Empty
            },
            TotalCount = 0
        };

        var json = JsonSerializer.Serialize(original, _options);
        var roundtrip = JsonSerializer.Deserialize<ResponseModel>(json, _options);

        roundtrip.Should().NotBeNull();
        roundtrip!.IsSuccess.Should().Be(original.IsSuccess);
        roundtrip.ResponseData.Version.Should().Be(original.ResponseData.Version);
        roundtrip.ResponseData.Type.Should().Be(original.ResponseData.Type);
        roundtrip.ResponseData.ServerVersion.Should().Be(original.ResponseData.ServerVersion);
    }

    [Fact]
    public void RoundTrip_ErrorResponse_MaintainsErrorDetails()
    {
        var original = new ResponseModel
        {
            IsSuccess = false,
            UserInfo = UserInfo.BlankUserInfo,
            ResponseData = new ApiResponse
            {
                IsSuccess = false,
                Version = "1.16.1",
                Type = "melodee",
                ServerVersion = "1.0.0",
                DataPropertyName = string.Empty,
                Error = new Error(70, "Data not found")
            },
            TotalCount = 0
        };

        var json = JsonSerializer.Serialize(original, _options);
        var roundtrip = JsonSerializer.Deserialize<ResponseModel>(json, _options);

        roundtrip.Should().NotBeNull();
        roundtrip!.IsSuccess.Should().BeFalse();
        roundtrip.ResponseData.Error.Should().NotBeNull();
        roundtrip.ResponseData.Error!.Code.Should().Be(70);
        roundtrip.ResponseData.Error.Message.Should().Be("Data not found");
    }

    [Fact]
    public void Read_IgnoresMetadataProperties_ExtractsOnlyDataProperties()
    {
        var json = """
        {
          "subsonic-response": {
            "status": "ok",
            "version": "1.16.1",
            "type": "melodee",
            "serverVersion": "1.0.0",
            "openSubsonic": true,
            "actualData": {
              "id": "123"
            }
          }
        }
        """;

        var result = JsonSerializer.Deserialize<ResponseModel>(json, _options);

        result!.ResponseData.DataPropertyName.Should().Be("actualData");
        result.ResponseData.Data.Should().NotBeNull();
    }

    [Fact]
    public void Read_MultipleDataProperties_UsesFirstNonMetadata()
    {
        var json = """
        {
          "subsonic-response": {
            "status": "ok",
            "version": "1.16.1",
            "firstData": { "id": "1" },
            "secondData": { "id": "2" }
          }
        }
        """;

        var result = JsonSerializer.Deserialize<ResponseModel>(json, _options);

        result!.ResponseData.DataPropertyName.Should().Be("firstData");
    }

    [Fact]
    public void Write_EmptyDataPropertyName_ProducesValidJson()
    {
        var responseModel = new ResponseModel
        {
            IsSuccess = true,
            UserInfo = UserInfo.BlankUserInfo,
            ResponseData = new ApiResponse
            {
                IsSuccess = true,
                Version = "1.16.1",
                Type = "melodee",
                ServerVersion = "1.0.0",
                DataPropertyName = string.Empty,
                Data = new { test = "value" }
            },
            TotalCount = 0
        };

        var json = JsonSerializer.Serialize(responseModel, _options);

        json.Should().Contain("subsonic-response");
        json.Should().Contain("status");
        json.Should().Contain("ok");
    }
}
