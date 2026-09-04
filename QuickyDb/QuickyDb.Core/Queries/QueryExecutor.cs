using QuickyDb.Core.Queries.Expressions;
using QuickyDb.Core.Queries.Expressions.Nodes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace QuickyDb.Core.Queries;

public sealed class QueryExecutor<TModel>
{
    private readonly IndexOperations<TModel> _primitives;

    public QueryExecutor(IndexOperations<TModel> primitives)
    {
        _primitives = primitives;
    }

    public IEnumerable<TModel> Search(Expression<Func<TModel, bool>> predicate)
    {
        var query = ExpressionParser.Parse(predicate);
        return Evaluate(query);
    }

    private HashSet<TModel> Evaluate(QueryNode node)
    {
        if (node is ConditionNode condition)
            return EvaluateCondition(condition);

        if (node is AndNode and)
        {
            var left = Evaluate(and.Left);
            left.IntersectWith(Evaluate(and.Right));
            return left;
        }

        if (node is OrNode or)
        {
            var left = Evaluate(or.Left);
            left.UnionWith(Evaluate(or.Right));
            return left;
        }

        if (node is NotNode not)
        {
            var universe = new HashSet<TModel>(_primitives.All());
            universe.ExceptWith(Evaluate(not.Inner));
            return universe;
        }

        throw new NotSupportedException(node.GetType().Name);
    }

    private HashSet<TModel> EvaluateCondition(ConditionNode condition)
    {
        var property = condition.Property;
        var result = new HashSet<TModel>();

        switch (condition.Operator)
        {
            case ExpressionType.Equal:
                var equal = _primitives.GetEqual(property, condition.Value);
                if (equal != null) result.UnionWith(equal);
                break;

            case ExpressionType.NotEqual:
                foreach (var group in _primitives.AllExcept(property, condition.Value))
                    result.UnionWith(group);
                break;

            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
                foreach (var group in _primitives.RangeUpTo(property, condition.Value, condition.Operator == ExpressionType.LessThanOrEqual))
                    result.UnionWith(group);
                break;

            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
                foreach (var group in _primitives.RangeFrom(property, condition.Value, condition.Operator == ExpressionType.GreaterThanOrEqual))
                    result.UnionWith(group);
                break;

            default:
                throw new NotSupportedException(condition.Operator.ToString());
        }

        return result;
    }
}