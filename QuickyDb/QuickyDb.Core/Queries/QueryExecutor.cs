using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using QuickyDb.Core.Queries.Expressions;
using QuickyDb.Core.Queries.Expressions.Nodes;

namespace QuickyDb.Core.Queries;

/// <summary>
/// Evaluates predicates over a table through the operations of <see cref="IndexOperations{TModel}"/>.
/// </summary>
/// <typeparam name="TModel">Type of the model.</typeparam>
public sealed class QueryExecutor<TModel>
{
    private readonly IndexOperations<TModel> _primitives;

    /// <summary>
    /// Creates an executor.
    /// </summary>
    /// <param name="primitives">Operations of the table to search in.</param>
    public QueryExecutor(IndexOperations<TModel> primitives)
    {
        _primitives = primitives;
    }

    /// <summary>
    /// <para>Finds the models that match the predicate.</para>
    /// <para>The predicate may compare indexed properties with values.</para>
    /// <para>The comparisons can be combined with AND, OR and NOT.</para>
    /// </summary>
    /// <param name="predicate">Condition that the models must satisfy.</param>
    /// <returns>Matching models. The order of the result is not defined.</returns>
    /// <exception cref="NotSupportedException">The predicate contains an unsupported expression.</exception>
    public IEnumerable<TModel> Search(Expression<Func<TModel, bool>> predicate)
    {
        QueryNodeBase query = ExpressionParser.Parse(predicate);
        return Evaluate(query);
    }

    // Каждая ветка возвращает новый набор, а не данные индекса, поэтому IntersectWith, UnionWith и ExceptWith
    // ниже можно применять к нему на месте: индексы при этом не портятся.
    private HashSet<TModel> Evaluate(QueryNodeBase node)
    {
        if (node is ConditionNode condition)
        {
            return EvaluateCondition(condition);
        }

        if (node is AndNode and)
        {
            HashSet<TModel> left = Evaluate(and.Left);
            left.IntersectWith(Evaluate(and.Right));
            return left;
        }

        if (node is OrNode or)
        {
            HashSet<TModel> left = Evaluate(or.Left);
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
        PropertyInfo property = condition.Property;
        var result = new HashSet<TModel>();

        switch (condition.Operator)
        {
            case ExpressionType.Equal:
                IReadOnlyList<TModel> equal = _primitives.GetEqual(property, condition.Value);
                if (equal != null)
                {
                    result.UnionWith(equal);
                }
                break;

            case ExpressionType.NotEqual:
                foreach (var group in _primitives.AllExcept(property, condition.Value))
                {
                    result.UnionWith(group);
                }
                break;

            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
                foreach (var group in _primitives.RangeUpTo(
                    property, condition.Value, condition.Operator == ExpressionType.LessThanOrEqual))
                {
                    result.UnionWith(group);
                }
                break;

            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
                foreach (var group in _primitives.RangeFrom(
                    property, condition.Value, condition.Operator == ExpressionType.GreaterThanOrEqual))
                {
                    result.UnionWith(group);
                }
                break;

            default:
                throw new NotSupportedException(condition.Operator.ToString());
        }

        return result;
    }
}