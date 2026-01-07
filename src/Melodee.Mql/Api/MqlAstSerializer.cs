using Melodee.Mql.Api.Dto;
using Melodee.Mql.Models;

namespace Melodee.Mql.Api;

/// <summary>
/// Converts MQL AST nodes to DTOs for API serialization.
/// </summary>
public static class MqlAstSerializer
{
    /// <summary>
    /// Converts an AST node to its DTO representation.
    /// </summary>
    public static MqlAstDto? ToDto(this MqlAstNode? node)
    {
        if (node == null)
        {
            return null;
        }

        return node switch
        {
            FreeTextNode freeText => new MqlFreeTextDto
            {
                NodeType = "FreeText",
                Text = freeText.Text
            },
            FieldExpressionNode field => new MqlFieldExpressionDto
            {
                NodeType = "FieldExpression",
                Field = field.Field,
                Operator = field.Operator,
                Value = field.Value?.ToString() ?? string.Empty
            },
            BinaryExpressionNode binary => new MqlBinaryExpressionDto
            {
                NodeType = "BinaryExpression",
                Operator = binary.Operator,
                Left = binary.Left.ToDto()!,
                Right = binary.Right.ToDto()!
            },
            UnaryExpressionNode unary => new MqlUnaryExpressionDto
            {
                NodeType = "UnaryExpression",
                Operand = unary.Operand.ToDto()!
            },
            GroupNode group => new MqlGroupDto
            {
                NodeType = "Group",
                Inner = group.Inner.ToDto()!
            },
            RangeNode range => new MqlRangeDto
            {
                NodeType = "Range",
                Field = range.Field,
                Min = range.Min?.ToString() ?? string.Empty,
                Max = range.Max?.ToString() ?? string.Empty
            },
            _ => null
        };
    }
}
