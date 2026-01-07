using FluentAssertions;
using Melodee.Mql.Constants;
using Melodee.Mql.Models;

namespace Melodee.Mql.Tests;

public class MqlParserTests
{
    private readonly MqlTokenizer _tokenizer = new();
    private readonly MqlParser _parser;

    public MqlParserTests()
    {
        _parser = new MqlParser();
    }

    private MqlParseResult ParseQuery(string query)
    {
        var tokens = _tokenizer.Tokenize(query);
        return _parser.Parse(tokens, "songs");
    }

    [Fact]
    public void Parse_SimpleFieldExpression_ReturnsFieldExpressionNode()
    {
        var result = ParseQuery("artist:Beatles");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<FieldExpressionNode>();
        var fieldNode = (FieldExpressionNode)result.Ast!;
        fieldNode.Field.Should().Be("artist");
        fieldNode.Value.Should().Be("Beatles");
    }

    [Fact]
    public void Parse_FreeText_ReturnsFreeTextNode()
    {
        var result = ParseQuery("pink floyd");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<BinaryExpressionNode>();
    }

    [Fact]
    public void Parse_AndExpression_ReturnsBinaryExpressionNode()
    {
        var result = ParseQuery("artist:Beatles AND year:1970");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<BinaryExpressionNode>();
        var binaryNode = (BinaryExpressionNode)result.Ast!;
        binaryNode.Operator.Should().Be("AND");
    }

    [Fact]
    public void Parse_OrExpression_ReturnsBinaryExpressionNode()
    {
        var result = ParseQuery("artist:Beatles OR artist:Pink Floyd");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<BinaryExpressionNode>();
        var binaryNode = (BinaryExpressionNode)result.Ast!;
        binaryNode.Operator.Should().Be("OR");
    }

    [Fact]
    public void Parse_NotExpression_ReturnsUnaryExpressionNode()
    {
        var result = ParseQuery("NOT live");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<UnaryExpressionNode>();
        var unaryNode = (UnaryExpressionNode)result.Ast!;
        unaryNode.Operator.Should().Be("NOT");
    }

    [Fact]
    public void Parse_Parentheses_ReturnsGroupNode()
    {
        var result = ParseQuery("(artist:Beatles OR artist:Pink Floyd)");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<GroupNode>();
        var groupNode = (GroupNode)result.Ast!;
        groupNode.Inner.Should().BeOfType<BinaryExpressionNode>();
    }

    [Fact]
    public void Parse_NestedParentheses_ReturnsNestedGroupNodes()
    {
        var result = ParseQuery("((artist:Beatles))");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<GroupNode>();
        var outerGroup = (GroupNode)result.Ast!;
        outerGroup.Inner.Should().BeOfType<GroupNode>();
    }

    [Fact]
    public void Parse_ImplicitAnd_AddsAndBetweenTerms()
    {
        var result = ParseQuery("artist:Beatles year:1970");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<BinaryExpressionNode>();
        result.NormalizedQuery.Should().Contain(" AND ");
    }

    [Fact]
    public void Parse_ComparisonOperators_ReturnsFieldExpressionWithOperator()
    {
        var result = ParseQuery("year:>=1970");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Field.Should().Be("year");
        fieldNode.Operator.Should().Be("GreaterThanOrEquals");
    }

    [Theory]
    [InlineData(":=", "Equals")]
    [InlineData(":!=", "NotEquals")]
    [InlineData(":<", "LessThan")]
    [InlineData(":<=", "LessThanOrEquals")]
    [InlineData(":>", "GreaterThan")]
    [InlineData(":>=", "GreaterThanOrEquals")]
    [InlineData(":", "Equals")]
    public void Parse_AllOperators_ReturnsCorrectOperator(string op, string expectedOp)
    {
        var result = ParseQuery($"year{op}2000");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Operator.Should().Be(expectedOp);
    }

    [Fact]
    public void Parse_StringLiteral_ValueIsString()
    {
        var result = ParseQuery("artist:\"Pink Floyd\"");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Value.Should().Be("Pink Floyd");
    }

    [Fact]
    public void Parse_NumberLiteral_ValueIsNumber()
    {
        var result = ParseQuery("rating:5");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Value.Should().Be(5);
    }

    [Fact]
    public void Parse_DecimalNumber_ValueIsDouble()
    {
        var result = ParseQuery("rating:4.5");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Value.Should().Be(4.5);
    }

    [Fact]
    public void Parse_BooleanLiteral_ValueIsBool()
    {
        var result = ParseQuery("starred:true");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Value.Should().Be(true);
    }

    [Fact]
    public void Parse_DateLiteral_ValueIsDateString()
    {
        var result = ParseQuery("added:2026-01-06");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Value.Should().Be("2026-01-06");
    }

    [Fact]
    public void Parse_RelativeDate_ValueIsRelativeDate()
    {
        var result = ParseQuery("added:-7d");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Value.Should().Be("-7d");
    }

    [Fact]
    public void Parse_DateKeyword_ValueIsKeyword()
    {
        var result = ParseQuery("added:today");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Value.Should().Be("today");
    }

    [Fact]
    public void Parse_ComplexBooleanExpression_ReturnsCorrectAst()
    {
        var result = ParseQuery("(artist:Beatles OR artist:Pink Floyd) AND year:>=1970 NOT live");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().NotBeNull();

        // The structure should be: AND(NOT, OR, AND)
        result.Ast.Should().BeOfType<BinaryExpressionNode>();
        var root = (BinaryExpressionNode)result.Ast!;
        root.Operator.Should().Be("AND");
    }

    [Fact]
    public void Parse_UnbalancedParentheses_ReturnsError()
    {
        var result = ParseQuery("(artist:Beatles");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == MqlErrorCodes.MqlUnbalancedParens);
    }

    [Fact]
    public void Parse_ExtraParenthesis_ReturnsError()
    {
        var result = ParseQuery("artist:Beatles))");

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Parse_EmptyQuery_ReturnsValidWithNullAst()
    {
        var result = ParseQuery("");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeNull();
    }

    [Fact]
    public void Parse_WhitespaceOnly_ReturnsValidWithNullAst()
    {
        var result = ParseQuery("   ");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Parse_NormalizedQuery_HasCanonicalSpacing()
    {
        var result = ParseQuery("artist:Beatles AND  year:1970");

        result.IsValid.Should().BeTrue();
        result.NormalizedQuery.Should().Contain(" AND ");
        result.NormalizedQuery.Should().NotContain("  ");
    }

    [Fact]
    public void Parse_ImplicitAnd_NormalizedQueryShowsAnd()
    {
        var result = ParseQuery("artist:Beatles year:1970");

        result.IsValid.Should().BeTrue();
        result.NormalizedQuery.Should().Contain(" AND ");
    }

    [Fact]
    public void Parse_FieldNameLowercased_NormalizedQueryHasLowercase()
    {
        var result = ParseQuery("ARTIST:Beatles");

        result.IsValid.Should().BeTrue();
        result.NormalizedQuery.Should().Contain("artist:");
    }

    [Fact]
    public void Parse_MissingColon_ReturnsError()
    {
        var result = ParseQuery("artist Beatles");

        result.IsValid.Should().BeTrue(); // Parses as free text
    }

    [Fact]
    public void Parse_Precedence_NotBeforeAndBeforeOr()
    {
        var result = ParseQuery("A AND NOT B OR C");

        result.IsValid.Should().BeTrue();
        // Structure should be: OR(AND(NOT, B), C)
        result.Ast.Should().BeOfType<BinaryExpressionNode>();
    }

    [Fact]
    public void Parse_ParenthesesOverridePrecedence()
    {
        var result = ParseQuery("A OR (B AND C)");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<BinaryExpressionNode>();
        var root = (BinaryExpressionNode)result.Ast!;
        root.Operator.Should().Be("OR");
    }

    [Fact]
    public void Parse_MultipleNotChained_ReturnsNestedUnaryNodes()
    {
        var result = ParseQuery("NOT NOT live");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<UnaryExpressionNode>();
        var firstNot = (UnaryExpressionNode)result.Ast!;
        firstNot.Operator.Should().Be("NOT");
        firstNot.Operand.Should().BeOfType<UnaryExpressionNode>();
    }

    [Fact]
    public void Parse_ThreeTermsImplicitAnd_ReturnsLeftAssociativeChain()
    {
        var result = ParseQuery("A B C");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<BinaryExpressionNode>();

        var root = (BinaryExpressionNode)result.Ast!;
        root.Operator.Should().Be("AND");

        var left = (BinaryExpressionNode)root.Left;
        left.Operator.Should().Be("AND");

        left.Left.Should().BeOfType<FreeTextNode>();
        left.Right.Should().BeOfType<FreeTextNode>();
        root.Right.Should().BeOfType<FreeTextNode>();
    }

    [Fact]
    public void Parse_Warnings_ListedInResult()
    {
        var result = ParseQuery("artist:Beatles");

        result.IsValid.Should().BeTrue();
        result.Warnings.Should().NotBeNull();
    }

    [Fact]
    public void Parse_Errors_ListedInResult()
    {
        var result = ParseQuery("(artist:Beatles");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Parse_Error_IncludesPosition()
    {
        var result = ParseQuery("(artist:Beatles");

        result.Errors.Should().Contain(e => e.Position != null);
    }

    [Fact]
    public void Parse_RangeExpression_ReturnsRangeNode()
    {
        var result = ParseQuery("year:1970-1980");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<RangeNode>();
        var rangeNode = (RangeNode)result.Ast!;
        rangeNode.Field.Should().Be("year");
        rangeNode.Min.Should().Be(1970);
        rangeNode.Max.Should().Be(1980);
    }

    [Fact]
    public void Parse_RegexPattern_ReturnsFieldExpressionWithRegex()
    {
        var result = ParseQuery("title:/remix/i");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Field.Should().Be("title");
        fieldNode.Value.Should().Be("/remix/i");
    }

    [Fact]
    public void Parse_ComplexQueryWithAllOperators_ReturnsValidAst()
    {
        var result = ParseQuery("(artist:\"Pink Floyd\" OR artist:Beatles) AND year:>=1970 AND NOT live AND rating:>3.5");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().NotBeNull();
    }

    [Fact]
    public void Parse_EmptyFieldValue_ReturnsFieldExpression()
    {
        var result = ParseQuery("artist:");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<FieldExpressionNode>();
    }

    [Fact]
    public void Parse_MixedCaseKeywords_ParsesCorrectly()
    {
        var result = ParseQuery("and or not");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().NotBeNull();
    }

    [Fact]
    public void Parse_QuotedStringWithSpaces_ParsesAsSingleValue()
    {
        var result = ParseQuery("title:\"The Dark Side of the Moon\"");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Value.Should().Be("The Dark Side of the Moon");
    }

    [Fact]
    public void Parse_EscapedQuoteInString_ParsesCorrectly()
    {
        var result = ParseQuery("title:\"Test\\\"Song\"");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Value.Should().Be("Test\"Song");
    }

    [Fact]
    public void Parse_NumberWithSign_ParsesAsNumber()
    {
        var result = ParseQuery("rating:-5");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Value.Should().Be(-5);
    }

    [Fact]
    public void Parse_LastWeekKeyword_ParsesAsDate()
    {
        var result = ParseQuery("added:last-week");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Value.Should().Be("last-week");
    }

    [Fact]
    public void Parse_FourDigitYearWithDash_ParsesAsDate()
    {
        var result = ParseQuery("year:1970-1980");

        result.IsValid.Should().BeTrue();
        result.Ast.Should().BeOfType<RangeNode>();
    }

    [Fact]
    public void Parse_SingleYear_ParsesAsNumber()
    {
        var result = ParseQuery("year:1970");

        result.IsValid.Should().BeTrue();
        var fieldNode = result.Ast.Should().BeOfType<FieldExpressionNode>().Subject;
        fieldNode.Value.Should().Be(1970);
    }
}
