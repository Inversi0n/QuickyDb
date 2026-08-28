using QuickyDb.Core.Common;
using QuickyDb.Core.Common.Attributes;
using QuickyDb.Core.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace QuickyDb.Rb
{
    public class RbTable<TModel>
    {
        public string Name { get; set; }

        private readonly Dictionary<string, IndexedStore<TModel>> _indexes;
        private readonly HashSet<TModel> _all;
        private readonly QueryExecutor<TModel> _query;

        public RbTable()
        {
            Name = typeof(TModel).Name;

            var properties = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var indexProperties = properties
                .Where(p => p.GetCustomAttributes(typeof(IndexAttribute), true)?.Length > 0)
                .ToArray();

            _indexes = new Dictionary<string, IndexedStore<TModel>>();
            foreach (var prop in indexProperties)
                _indexes.Add(prop.Name, new IndexedStore<TModel>(prop));

            _all = new HashSet<TModel>();

            _query = new QueryExecutor<TModel>(new IndexOperations<TModel>
            {
                All = () => _all,
                GetEqual = (prop, value) =>
                    GetIndex(prop).TryGetEqual(ByteEncoder.Encode(value), out var list) ? list : null,
                RangeUpTo = (prop, bound, inclusive) =>
                    GetIndex(prop).RangeUpTo(ByteEncoder.Encode(bound), inclusive),
                RangeFrom = (prop, bound, inclusive) =>
                    GetIndex(prop).RangeFrom(ByteEncoder.Encode(bound), inclusive),
                AllExcept = (prop, value) =>
                    GetIndex(prop).AllExcept(ByteEncoder.Encode(value)),
            });
        }

        public IEnumerable<TModel> Search(Expression<Func<TModel, bool>> predicate) => _query.Search(predicate);

        public void Add(TModel model)
        {
            _all.Add(model);
            foreach (var index in _indexes.Values)
                index.Add(model);
        }

        public void Remove(TModel model)
        {
            _all.Remove(model);
            foreach (var index in _indexes.Values)
                index.Remove(model);
        }

        private IndexedStore<TModel> GetIndex(PropertyInfo property)
        {
            if (!_indexes.TryGetValue(property.Name, out var index))
                throw new InvalidOperationException($"Свойство {property.Name} не проиндексировано (нет [Index]).");
            return index;
        }
    }
}