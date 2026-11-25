using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using QuickyDb.Core.Common;
using QuickyDb.Core.Common.Attributes;
using QuickyDb.Core.Queries;
using QuickyDb.Rb.IndexedStore;

namespace QuickyDb.Rb;

/// <summary>
/// <para>In-memory table that keeps its models in sorted indexes built on red-black trees.</para>
/// <para>Every property marked with <see cref="IndexAttribute"/> gets its own index.</para>
/// <para>Properties marked with <see cref="CascadeIndexAttribute"/> are grouped into composite indexes by name.</para>
/// <para><see cref="Search"/> can filter only by properties that have an index of their own.</para>
/// </summary>
/// <typeparam name="TModel">Type of the model.</typeparam>
public class RbTable<TModel>
{
    /// <summary>
    /// Name of the table. By default it is the name of the model type.
    /// </summary>
    public string Name { get; set; }

    private readonly Dictionary<string, IndexedStoreBase<TModel>> _indexes;
    private readonly HashSet<TModel> _all;
    private readonly QueryExecutor<TModel> _query;

    /// <summary>
    /// Creates an empty table and its indexes from the attributes of the model properties.
    /// </summary>
    /// <exception cref="InvalidOperationException">A property marked for indexing is a collection.</exception>
    public RbTable()
    {
        Name = typeof(TModel).Name;

        PropertyInfo[] properties = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        PropertyInfo[] indexProperties = properties
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
                GetIndex(prop).TryGetEqual(ByteEncoder.Encode(value), out List<TModel> list) ? list : null,
            RangeUpTo = (prop, bound, inclusive) =>
                GetIndex(prop).RangeUpTo(ByteEncoder.Encode(bound), inclusive),
            RangeFrom = (prop, bound, inclusive) =>
                GetIndex(prop).RangeFrom(ByteEncoder.Encode(bound), inclusive),
            AllExcept = (prop, value) =>
                GetIndex(prop).AllExcept(ByteEncoder.Encode(value)),
        });

        IGrouping<string, PropertyInfo>[] cascadeProperties = properties
            .Where(p => p.GetCustomAttributes(typeof(CascadeIndexAttribute), true)?.Length > 0)
            .Select(p => (p, p.GetCustomAttribute<CascadeIndexAttribute>()))
            .OrderBy(pair => pair.Item2.Order)
            .GroupBy(p => p.Item2.Name, p => p.p)
            .ToArray();

        foreach (var cascade in cascadeProperties)
        {
            foreach (var prop in cascade)
            {
                ValidateIndexablePropertyType(prop);
            }
            _indexes.Add(cascade.Key, new CascadeIndexedStore<TModel>(cascade.ToArray()));
        }
    }

    private static void ValidateIndexablePropertyType(PropertyInfo property)
    {
        Type type = property.PropertyType;
        bool isCollection = type != typeof(string)
            && typeof(System.Collections.IEnumerable).IsAssignableFrom(type);

        if (isCollection)
        {
            throw new InvalidOperationException(
                $"Свойство {property.Name} — коллекция ({type.Name}); индекс (обычный или составной) " +
                "по коллекциям пока не поддерживается: нет естественного полного порядка для memcmp-сравнения.");
        }
    }

    private IndexedStoreBase<TModel> GetIndex(PropertyInfo property)
    {
        // TODO (Mikhail): определять, что запрос идёт по составному индексу.
        if (!_indexes.TryGetValue(property.Name, out IndexedStoreBase<TModel> index))
        {
            throw new InvalidOperationException($"Свойство {property.Name} не проиндексировано (нет [Index]).");
        }
        return index;
    }

    /// <summary>
    /// Finds the models that match the predicate.
    /// </summary>
    /// <param name="predicate">
    /// Comparisons of indexed properties with values, combined with logical AND, OR and NOT.
    /// </param>
    /// <returns>Matching models, in no particular order.</returns>
    /// <exception cref="InvalidOperationException">
    /// The predicate uses a property that has no index of its own.
    /// </exception>
    /// <exception cref="NotSupportedException">The predicate contains an unsupported expression.</exception>
    public IEnumerable<TModel> Search(Expression<Func<TModel, bool>> predicate) => _query.Search(predicate);

    /// <summary>
    /// <para>Adds the model to the table and to all indexes.</para>
    /// <para>The indexed properties of the model must not be changed while it is in the table.</para>
    /// </summary>
    /// <param name="model">Model to add.</param>
    public void Add(TModel model)
    {
        _all.Add(model);
        foreach (var index in _indexes.Values)
        {
            index.Add(model);
        }
    }

    /// <summary>
    /// <para>Removes the model from the table and from all indexes.</para>
    /// <para>Does nothing if the model is not in the table.</para>
    /// </summary>
    /// <param name="model">Model to remove.</param>
    public void Remove(TModel model)
    {
        _all.Remove(model);
        foreach (var index in _indexes.Values)
        {
            index.Remove(model);
        }
    }
}