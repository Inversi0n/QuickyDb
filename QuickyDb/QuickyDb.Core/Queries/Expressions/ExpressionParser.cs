using QuickyDb.Core.Queries.Expressions.Nodes;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace QuickyDb.Core.Queries.Expressions;

public static class ExpressionParser
{
    public static QueryNode Parse<TModel>(Expression<Func<TModel, bool>> predicate)
    {
        return ParseNode(predicate.Body, predicate.Parameters[0]);
    }

    private static QueryNode ParseNode(Expression expr, ParameterExpression param)
    {
        expr = StripConvert(expr);

        if (expr is BinaryExpression binary)
        {
            if (binary.NodeType == ExpressionType.AndAlso)
                return new AndNode(ParseNode(binary.Left, param), ParseNode(binary.Right, param));

            if (binary.NodeType == ExpressionType.OrElse)
                return new OrNode(ParseNode(binary.Left, param), ParseNode(binary.Right, param));

            if (IsComparison(binary.NodeType))
                return ParseComparison(binary, param);
        }

        if (expr is UnaryExpression unary && unary.NodeType == ExpressionType.Not)
            return new NotNode(ParseNode(unary.Operand, param));

        throw new NotSupportedException($"Expression not supported: {expr}");
    }

    private static bool IsComparison(ExpressionType type)
    {
        return type == ExpressionType.Equal
            || type == ExpressionType.NotEqual
            || type == ExpressionType.LessThan
            || type == ExpressionType.LessThanOrEqual
            || type == ExpressionType.GreaterThan
            || type == ExpressionType.GreaterThanOrEqual;
    }

    private static ConditionNode ParseComparison(BinaryExpression expr, ParameterExpression param)
    {
        var left = StripConvert(expr.Left);
        var right = StripConvert(expr.Right);

        if (TryGetProperty(left, param, out var property))
            return new ConditionNode(property, expr.NodeType, EvaluateConstant(right));

        if (TryGetProperty(right, param, out property))
            return new ConditionNode(property, Flip(expr.NodeType), EvaluateConstant(left));

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
            return constant.Value;

        // значение из замыкания, поля, вызова метода и т.п.
        return Expression.Lambda(expr).Compile().DynamicInvoke();
    }

    private static Expression StripConvert(Expression expr)
    {
        while (expr.NodeType == ExpressionType.Convert || expr.NodeType == ExpressionType.ConvertChecked)
            expr = ((UnaryExpression)expr).Operand;
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
            default: return type; // Equal / NotEqual симметричны
        }
    }
}