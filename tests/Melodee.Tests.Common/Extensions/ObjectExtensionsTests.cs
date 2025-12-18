using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Extensions;

public class ObjectExtensionsTests
{
    [Fact]
    public void IsEnumerable_WithArray_ReturnsTrue()
    {
        object obj = new[] { 1, 2, 3 };
        Assert.True(obj.IsEnumerable());
    }

    [Fact]
    public void IsEnumerable_WithList_ReturnsTrue()
    {
        object obj = new List<string> { "a", "b" };
        Assert.True(obj.IsEnumerable());
    }

    [Fact]
    public void IsEnumerable_WithString_ReturnsTrue()
    {
        // String implements IEnumerable<char>
        object obj = "hello";
        Assert.True(obj.IsEnumerable());
    }

    [Fact]
    public void IsEnumerable_WithInt_ReturnsFalse()
    {
        object obj = 42;
        Assert.False(obj.IsEnumerable());
    }

    [Fact]
    public void ToDictionary_SimpleObject_ReturnsProperties()
    {
        var obj = new TestClass { Name = "Test", Value = 42 };
        var result = obj.ToDictionary();

        Assert.Equal("Test", result["Name"]);
        Assert.Equal(42, result["Value"]);
    }

    [Fact]
    public void Convert_NullableTypeWithNull_ReturnsNull()
    {
        object? value = null;
        var result = value.Convert(typeof(int?));
        Assert.Null(result);
    }

    [Fact]
    public void Convert_IntToDouble_Converts()
    {
        object value = 42;
        var result = value.Convert(typeof(double));
        Assert.IsType<double>(result);
        Assert.Equal(42.0, result);
    }

    [Fact]
    public void Convert_StringToInt_Converts()
    {
        object value = "123";
        var result = value.Convert(typeof(int));
        Assert.IsType<int>(result);
        Assert.Equal(123, result);
    }

    [Fact]
    public void ConvertGeneric_IntToDouble_Converts()
    {
        object value = 42;
        var result = value.Convert<double>();
        Assert.Equal(42.0, result);
    }

    [Fact]
    public void ConvertGeneric_NullValueForReferenceType_ReturnsNull()
    {
        object? value = null;
        var result = value.Convert<string>();
        Assert.Null(result);
    }

    [Fact]
    public void ConvertGeneric_NonConvertible_ReturnsCast()
    {
        var original = new TestClass { Name = "Test" };
        object value = original;
        var result = value.Convert<TestClass>();
        Assert.Same(original, result);
    }

    [Theory]
    [InlineData((byte)1, true)]
    [InlineData((sbyte)1, true)]
    [InlineData((short)1, true)]
    [InlineData((ushort)1, true)]
    [InlineData(1, true)]
    [InlineData(1u, true)]
    [InlineData(1L, true)]
    [InlineData(1ul, true)]
    [InlineData(1.0f, true)]
    [InlineData(1.0d, true)]
    [InlineData("string", false)]
    public void IsNumericType_ReturnsExpected(object value, bool expected)
    {
        Assert.Equal(expected, value.IsNumericType());
    }

    [Fact]
    public void IsNumericType_Decimal_ReturnsTrue()
    {
        object value = 1.0m;
        Assert.True(value.IsNumericType());
    }

    [Fact]
    public void IsNullOrDefault_Null_ReturnsTrue()
    {
        string? value = null;
        Assert.True(value.IsNullOrDefault());
    }

    [Fact]
    public void IsNullOrDefault_DefaultInt_ReturnsTrue()
    {
        int value = 0;
        Assert.True(value.IsNullOrDefault());
    }

    [Fact]
    public void IsNullOrDefault_NonDefaultInt_ReturnsFalse()
    {
        int value = 42;
        Assert.False(value.IsNullOrDefault());
    }

    [Fact]
    public void IsNullOrDefault_DefaultDateTime_ReturnsTrue()
    {
        DateTime value = default;
        Assert.True(value.IsNullOrDefault());
    }

    [Fact]
    public void IsNullOrDefault_NonDefaultDateTime_ReturnsFalse()
    {
        DateTime value = DateTime.Now;
        Assert.False(value.IsNullOrDefault());
    }

    [Fact]
    public void IsNullOrDefault_NullableIntWithValue_ReturnsFalse()
    {
        int? value = 42;
        Assert.False(value.IsNullOrDefault());
    }

    [Fact]
    public void IsNullOrDefault_EmptyString_ReturnsFalse()
    {
        // Empty string is not the same as default(string) which is null
        string value = "";
        Assert.False(value.IsNullOrDefault());
    }

    private class TestClass
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }
}
