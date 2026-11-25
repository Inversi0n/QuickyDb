using System;
using System.Linq.Expressions;
using System.Reflection;
using QuickyDb.Core.Queries.Expressions.Nodes;

namespace QuickyDb.Core.Queries.Expressions;

/// <summary>
/// Converts predicate expressions into trees of <see cref="QueryNode"/>.
/// </summary>
public static class ExpressionParser
{
    /// <summary>
    /// <para>Converts a predicate into a tree of query nodes.</para>
    /// <para>Supported are comparisons of a model property with a value, combined with logical AND, OR and NOT.</para>
    /// <para>The value may be a constant, a captured variable or any expression that does not use the model.</para>
    /// </summary>
    /// <typeparam name="TModel">Type of the model.</typeparam>
    /// <param name="predicate">Predicate to convert.</param>
    /// <returns>Root node of the tree.</returns>
    /// <exception cref="NotSupportedException">
    /// The predicate contains an unsupported expression, or a comparison without a model property.
    /// </exception>
    public static QueryNode Parse<TModel>(Expression<Func<TModel, bool>> predicate)
        => ParseNode(predicate.Body, predicate.Parameters[0]);

    private static QueryNode ParseNode(Expression expr, ParameterExpression param)
    {
        expr = StripConvert(expr);

        if (expr is BinaryExpression binary)
        {
            if (binary.NodeType == ExpressionType.AndAlso)
            {
                return new AndNode(ParseNode(binary.Left, param), ParseNode(binary.Right, param));
            }

            if (binary.NodeType == ExpressionType.OrElse)
            {
                return new OrNode(ParseNode(binary.Left, param), ParseNode(binary.Right, param));
            }

            if (IsComparison(binary.NodeType))
            {
                return ParseComparison(binary, param);
            }
        }

        if (expr is UnaryExpression unary && unary.NodeType == ExpressionType.Not)
        {
            return new NotNode(ParseNode(unary.Operand, param));
        }

        throw new NotSupportedException($"Expression not supported: {expr}");
    }

    private static bool IsComparison(ExpressionType type)
        => type == ExpressionType.Equal
            || type == ExpressionType.NotEqual
            || type == ExpressionType.LessThan
            || type == ExpressionType.LessThanOrEqual
            || type == ExpressionType.GreaterThan
            || type == ExpressionType.GreaterThanOrEqual;

    private static ConditionNode ParseComparison(BinaryExpression expr, ParameterExpression param)
    {
        Expression left = StripConvert(expr.Left);
        Expression right = StripConvert(expr.Right);

        if (TryGetProperty(left, param, out PropertyInfo property))
        {
            return new ConditionNode(property, expr.NodeType, EvaluateConstant(right));
        }

        if (TryGetProperty(right, param, out property))
        {
            return new ConditionNode(property, Flip(expr.NodeType), EvaluateConstant(left));
        }

        throw new NotSupportedException($"There is no reference to the model property in response: {expr}");
    }

    private static bool TryGetProperty(Expression expr, ParameterExpression param, out PropertyInfo property)
    {
        if (expr is MemberExpression member && member.Expression == param && member.Member is PropertyInfo info)
        {
            property = info;
            return true;
        }
        property = null;
        return false;
    }

    private static object EvaluateConstant(Expression expr)
    {
        if (expr is ConstantExpression constant)
        {
            return constant.Value;
        }

        // Значение из замыкания, поля, вызова метода и т.п. вычисляется один раз, при разборе предиката.
        return Expression.Lambda(expr).Compile().DynamicInvoke();
    }

    // Снимает приведения типов, чтобы добраться до свойства или значения под ними (например, int к object).
    // Подводный камень: значение при этом не приводится к типу свойства. Для свойства double и значения int
    // ключ закодируется как int (4 байта), а в индексе лежат 8-байтовые ключи, поэтому результат будет неверным.
    private static Expression StripConvert(Expression expr)
    {
        while (expr.NodeType == ExpressionType.Convert || expr.NodeType == ExpressionType.ConvertChecked)
        {
            expr = ((UnaryExpression)expr).Operand;
        }
        return expr;
    }

    private static ExpressionType Flip(ExpressionType type)
    {
        switch (type)
        {
            case ExpressionType.LessThan: return ExpressionType.GreaterThan;
            case ExpressionType.LessThanOrEqual: return ExpressionType.GreaterThanOrEqual;
            case ExpressionType.GreaterThan: return ExpressionType.LessThan;
            case ExpressionType.GreaterThanOrEqual: return ExpressionType.LessThanOrEqual;
            // Equal и NotEqual симметричны, поэтому остаются без изменений.
            default: return type;
        }
    }
}