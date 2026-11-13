using QuickyDb.Core.Queries.Expressions;
using QuickyDb.Core.Queries.Expressions.Nodes;
using System.Linq.Expressions;

namespace QuickyDb.Tests.Core.Queries.Expressions;

[TestFixture]
internal class ExpressionParserTests
{
    private class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    [Test]
    public void Parse_SimpleEquality_ReturnsConditionNode()
    {
        Expression<Func<Item, bool>> predicate = i => i.Id == 5;
        var node = ExpressionParser.Parse(predicate);

        var condition = node as ConditionNode;
        Assert.That(condition, Is.Not.Null);
        Assert.That(condition!.Property.Name, Is.EqualTo(nameof(Item.Id)));
        Assert.That(condition.Operator, Is.EqualTo(ExpressionType.Equal));
        Assert.That(condition.Value, Is.EqualTo(5));
    }

    [Test]
    public void Parse_ConstantOnLeftSide_FlipsOperatorAndKeepsSemantics()
    {
        // "5 < i.Id" эквивалентно "i.Id > 5" — паттерн должен вернуть именно это.
        Expression<Func<Item, bool>> predicate = i => 5 < i.Id;
        var condition = (ConditionNode)ExpressionParser.Parse(predicate);

        Assert.That(condition.Operator, Is.EqualTo(ExpressionType.GreaterThan));
        Assert.That(condition.Value, Is.EqualTo(5));
    }

    [Test]
    public void Parse_ConstantOnLeftSide_LessThanOrEqualFlipsToGreaterThanOrEqual()
    {
        Expression<Func<Item, bool>> predicate = i => 10 <= i.Id; // i.Id >= 10
        var condition = (ConditionNode)ExpressionParser.Parse(predicate);

        Assert.That(condition.Operator, Is.EqualTo(ExpressionType.GreaterThanOrEqual));
        Assert.That(condition.Value, Is.EqualTo(10));
    }

    [Test]
    public void Parse_AndAlso_ReturnsAndNodeWithBothConditions()
    {
        Expression<Func<Item, bool>> predicate = i => i.Id > 0 && i.Name == "x";
        var node = ExpressionParser.Parse(predicate);

        var and = node as AndNode;
        Assert.That(and, Is.Not.Null);
        Assert.That(and!.Left, Is.InstanceOf<ConditionNode>());
        Assert.That(and.Right, Is.InstanceOf<ConditionNode>());
    }

    [Test]
    public void Parse_OrElse_ReturnsOrNode()
    {
        Expression<Func<Item, bool>> predicate = i => i.Id == 1 || i.Id == 2;
        var node = ExpressionParser.Parse(predicate);

        Assert.That(node, Is.InstanceOf<OrNode>());
    }

    [Test]
    public void Parse_Not_ReturnsNotNodeWrappingInner()
    {
        Expression<Func<Item, bool>> predicate = i => !(i.Id == 5);
        var node = ExpressionParser.Parse(predicate);

        var not = node as NotNode;
        Assert.That(not, Is.Not.Null);
        Assert.That(not!.Inner, Is.InstanceOf<ConditionNode>());
    }

    [Test]
    public void Parse_ClosureVariable_EvaluatesCapturedValue()
    {
        int threshold = 10;
        Expression<Func<Item, bool>> predicate = i => i.Id > threshold;
        var condition = (ConditionNode)ExpressionParser.Parse(predicate);

        Assert.That(condition.Value, Is.EqualTo(10));
    }

    [Test]
    public void Parse_NoPropertyReference_ThrowsNotSupported()
    {
        Expression<Func<Item, bool>> predicate = i => 1 == 1;
        Assert.Throws<NotSupportedException>(() => ExpressionParser.Parse(predicate));
    }

    [Test]
    public void Parse_UnsupportedExpressionShape_ThrowsNotSupported()
    {
        Expression<Func<Item, bool>> predicate = i => i.Name.Contains("x");
        Assert.Throws<NotSupportedException>(() => ExpressionParser.Parse(predicate));
    }
}
