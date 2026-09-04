using QuickyDb.Core.Common;
using QuickyDb.Core.Common.Attributes;
using QuickyDb.Core.Queries;
using QuickyDb.Rb.IndexedStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace QuickyDb.Rb;

public class RbTable<TModel>
{
    public string Name { get; set; }

    private readonly Dictionary<string, IndexedStoreBase<TModel>> _indexes;
    private readonly HashSet<TModel> _all;
    private readonly QueryExecutor<TModel> _query;

    public RbTable()
    {
        Name = typeof(TModel).Name;

        var properties = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var indexProperties = properties
            .Where(p => p.GetCustomAttributes(typeof(IndexAttribute), true)?.Length > 0)
            .ToArray();

        _indexes = new Dictionary<string, IndexedStoreBase<TModel>>();
        foreach (var prop in indexProperties)
        {
            ValidateIndexablePropertyType(prop);
            _indexes.Add(prop.Name, new SimpleIndexedStore<TModel>(prop));
        }

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


        var cascadeProperties = properties
         .Where(p => p.GetCustomAttributes(typeof(CascadeIndexAttribute), true)?.Length > 0)
         .Select(p => (p, p.GetCustomAttribute<CascadeIndexAttribute>()))
         .OrderBy(pair => pair.Item2.Order)
         .GroupBy(p => p.Item2.Name, p => p.p)
         .ToArray();

        foreach (var cascade in cascadeProperties)
        {
            foreach (var prop in cascade)
                ValidateIndexablePropertyType(prop);
            _indexes.Add(cascade.Key, new CascadeIndexedStore<TModel>(cascade.ToArray()));
        }
    }

    private static void ValidateIndexablePropertyType(PropertyInfo property)
    {
        var type = property.PropertyType;
        bool isCollection = type != typeof(string)
            && typeof(System.Collections.IEnumerable).IsAssignableFrom(type);

        if (isCollection)
            throw new InvalidOperationException(
                $"Свойство {property.Name} — коллекция ({type.Name}); индекс (обычный или составной) " +
                "по коллекциям пока не поддерживается: нет естественного полного порядка для memcmp-сравнения.");
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

    private IndexedStoreBase<TModel> GetIndex(PropertyInfo property)
    {
        //TODO need to undestand we're filtering by compsite index
        if (!_indexes.TryGetValue(property.Name, out var index))
            throw new InvalidOperationException($"Свойство {property.Name} не проиндексировано (нет [Index]).");
        return index;
    }
}