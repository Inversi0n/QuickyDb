using QuickyTree.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace QuickyDbRb
{
    public class RbTreeSet<TModel>
    {
        public string Name { get; set; }

        private readonly Dictionary<PropertyInfo, SortedSet<TModel>> _indexes;

        public RbTreeSet()
        {
            Name = typeof(TModel).Name;


            var properties = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var indexProperties = properties.Where(p => p.GetCustomAttributes(typeof(IndexAttribute), true)?.Length > 0).ToArray();

            _indexes = new Dictionary<PropertyInfo, SortedSet<TModel>>();
            foreach (var prop in indexProperties)
            {
                //(e1, e2) => (prop.GetValue(e1) as IComparable).CompareTo(prop.GetValue(e2) as IComparable)

                var propComparer = Comparer<TModel>.Create((e1, e2) =>
                {
                    var res = (prop.GetValue(e1) as IComparable).CompareTo(prop.GetValue(e2) as IComparable);
                    if (res != 0)
                        return res;
                    return e1.GetHashCode().CompareTo(e2.GetHashCode());
                });

                _indexes.Add(prop, new SortedSet<TModel>(propComparer));
            }
        }

        public IEnumerable<TModel> Search(Expression<Func<TModel, bool>> predicate)
        {
            var query = PredicateParser.Parse(predicate);
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
                var universe = new HashSet<TModel>(_indexes.Values.First());
                universe.ExceptWith(Evaluate(not.Inner));
                return universe;
            }

            throw new NotSupportedException(node.GetType().Name);
        }

        private HashSet<TModel> EvaluateCondition(ConditionNode condition)
        {
            if (!_indexes.TryGetValue(condition.Property, out var index))
                throw new InvalidOperationException(
                    $"Свойство {condition.Property.Name} не проиндексировано (нет [Index]).");

            var result = new HashSet<TModel>();


            foreach (var item in index)
            {
                var itemValue = (IComparable)condition.Property.GetValue(item);
                int cmp = itemValue.CompareTo(condition.Value);
                if (Matches(cmp, condition.Operator))
                    result.Add(item);
            }
            return result;
        }

        private static bool Matches(int cmp, ExpressionType op)
        {
            switch (op)
            {
                case ExpressionType.Equal: return cmp == 0;
                case ExpressionType.NotEqual: return cmp != 0;
                case ExpressionType.LessThan: return cmp < 0;
                case ExpressionType.LessThanOrEqual: return cmp <= 0;
                case ExpressionType.GreaterThan: return cmp > 0;
                case ExpressionType.GreaterThanOrEqual: return cmp >= 0;
                default: throw new NotSupportedException(op.ToString());
            }
        }

        public void Add(TModel model)
        {
            foreach (var index in _indexes)
            {
                index.Value.Add(model);
            }
        }

        public void Remove(TModel model)
        {
            foreach (var index in _indexes)
            {
                index.Value.Remove(model);
            }
        }
    }

}
