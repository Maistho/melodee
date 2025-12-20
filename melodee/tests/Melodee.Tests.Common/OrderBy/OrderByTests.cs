using System.Linq.Expressions;
using Melodee.Common.OrderBy;

namespace Melodee.Tests.Common.OrderBy;

public class OrderByTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    [Fact]
    public void OrderBy_WithPropertyExpression_StoresExpression()
    {
        var orderBy = new OrderBy<TestEntity, int>(x => x.Id);
        Assert.NotNull(orderBy.Expression);
    }

    [Fact]
    public void OrderBy_Expression_CanBeUsedForOrdering()
    {
        var orderBy = new OrderBy<TestEntity, string>(x => x.Name);
        Expression<Func<TestEntity, string>> expression = orderBy.Expression;

        var entities = new List<TestEntity>
        {
            new() { Id = 1, Name = "Charlie" },
            new() { Id = 2, Name = "Alice" },
            new() { Id = 3, Name = "Bob" }
        };

        var sorted = entities.AsQueryable().OrderBy(expression).ToList();

        Assert.Equal("Alice", sorted[0].Name);
        Assert.Equal("Bob", sorted[1].Name);
        Assert.Equal("Charlie", sorted[2].Name);
    }

    [Fact]
    public void OrderBy_WithDateTimeExpression_Works()
    {
        var orderBy = new OrderBy<TestEntity, DateTime>(x => x.CreatedAt);
        Assert.NotNull(orderBy.Expression);
    }

    [Fact]
    public void IOrderBy_ExpressionProperty_ReturnsDynamic()
    {
        IOrderBy orderBy = new OrderBy<TestEntity, int>(x => x.Id);
        Assert.NotNull(orderBy.Expression);
    }
}
