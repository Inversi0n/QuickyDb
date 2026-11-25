using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using QuickyDb.Core.Common;

namespace QuickyDb.Rb.IndexedStore;

/// <summary>
/// <para>Base class of a sorted in-memory index over one key.</para>
/// <para>Models with equal keys are stored together in one group.</para>
/// <para>Keys are compared by <see cref="ByteArrayComparer"/>.</para>
/// <para>The lists returned by the query methods belong to the store and must not be modified.</para>
/// <para>The range and scan methods are lazy, so do not change the store while their results are enumerated.</para>
/// </summary>
/// <typeparam name="TModel">Type of the model.</typeparam>
internal abstract class IndexedStoreBase<TModel>
{
    protected Func<TModel, byte[]> GetKeys;

    private readonly SortedSet<KeyGroup<TModel>> _entries;

    public IndexedStoreBase()
    {
        var comparer = Comparer<KeyGroup<TModel>>.Create(
            (a, b) => ByteArrayComparer.Instance.Compare(a.Key, b.Key));
        _entries = new SortedSet<KeyGroup<TModel>>(comparer);
    }

    /// <summary>
    /// Adds the model under the key that <see cref="GetKeys"/> produces for it.
    /// </summary>
    /// <param name="model">Model to add.</param>
    public void Add(TModel model)
    {
        byte[] key = GetKeys(model);
        var probe = new KeyGroup<TModel>(key);

        if (!_entries.TryGetValue(probe, out KeyGroup<TModel> group))
        {
            group = probe;
            _entries.Add(group);
        }
        group.Models.Add(model);
    }

    // Ключ пересчитывается по модели, а не берётся из сохранённого. Если проиндексированное свойство изменили
    // после Add, группа не найдётся, и модель останется в индексе.
    /// <summary>
    /// <para>Removes the model from its group, and removes the group when it becomes empty.</para>
    /// <para>Does nothing if there is no group for the key of the model.</para>
    /// </summary>
    /// <param name="model">Model to remove.</param>
    public void Remove(TModel model)
    {
        byte[] key = GetKeys(model);
        var probe = new KeyGroup<TModel>(key);
        if (!_entries.TryGetValue(probe, out KeyGroup<TModel> group))
        {
            return;
        }

        group.Models.Remove(model);
        if (group.Models.Count == 0)
        {
            _entries.Remove(group);
        }
    }

    /// <summary>
    /// Finds the models stored under exactly this key.
    /// </summary>
    /// <param name="key">Encoded key.</param>
    /// <param name="models">The models of the group, or null if the key is not found.</param>
    /// <returns>True if the key is found.</returns>
    public bool TryGetEqual(byte[] key, out List<TModel> models)
    {
        var probe = new KeyGroup<TModel>(key);
        if (_entries.TryGetValue(probe, out KeyGroup<TModel> group))
        {
            models = group.Models;
            return true;
        }
        models = null;
        return false;
    }

    /// <summary>
    /// Returns the groups whose keys are less than the bound, in ascending order of keys.
    /// </summary>
    /// <param name="bound">Encoded upper bound.</param>
    /// <param name="inclusive">True to include the group whose key is equal to the bound.</param>
    /// <returns>Models of every matching group.</returns>
    public IEnumerable<List<TModel>> RangeUpTo(byte[] bound, bool inclusive)
    {
        if (_entries.Count == 0)
        {
            yield break;
        }

        KeyGroup<TModel> min = _entries.Min;
        if (ByteArrayComparer.Instance.Compare(min.Key, bound) > 0)
        {
            yield break;
        }

        var probe = new KeyGroup<TModel>(bound);
        foreach (var group in _entries.GetViewBetween(min, probe))
        {
            if (!inclusive && ByteArrayComparer.Instance.Equals(group.Key, bound))
            {
                continue;
            }
            yield return group.Models;
        }
    }

    /// <summary>
    /// Returns the groups whose keys are greater than the bound, in ascending order of keys.
    /// </summary>
    /// <param name="bound">Encoded lower bound.</param>
    /// <param name="inclusive">True to include the group whose key is equal to the bound.</param>
    /// <returns>Models of every matching group.</returns>
    public IEnumerable<List<TModel>> RangeFrom(byte[] bound, bool inclusive)
    {
        if (_entries.Count == 0)
        {
            yield break;
        }

        KeyGroup<TModel> max = _entries.Max;
        if (ByteArrayComparer.Instance.Compare(max.Key, bound) < 0)
        {
            yield break;
        }

        var probe = new KeyGroup<TModel>(bound);
        foreach (var group in _entries.GetViewBetween(probe, max))
        {
            if (!inclusive && ByteArrayComparer.Instance.Equals(group.Key, bound))
            {
                continue;
            }
            yield return group.Models;
        }
    }

    /// <summary>
    /// Returns all groups except the one stored under the given key, in ascending order of keys.
    /// </summary>
    /// <param name="excludedKey">Encoded key of the group to skip.</param>
    /// <returns>Models of every other group.</returns>
    public IEnumerable<List<TModel>> AllExcept(byte[] excludedKey)
    {
        foreach (var group in _entries)
        {
            if (ByteArrayComparer.Instance.Equals(group.Key, excludedKey))
            {
                continue;
            }
            yield return group.Models;
        }
    }

    /// <summary>
    /// <para>Returns the groups whose keys start with the prefix, in ascending order of keys.</para>
    /// <para>An empty prefix returns nothing.</para>
    /// <para>Intended for lookups by the leading components of a composite key.</para>
    /// </summary>
    /// <param name="prefix">Encoded beginning of the key.</param>
    /// <returns>Models of every matching group.</returns>
    public IEnumerable<List<TModel>> PrefixScan(byte[] prefix)
    {
        if (_entries.Count == 0 || prefix.Length == 0)
        {
            yield break;
        }

        KeyGroup<TModel> max = _entries.Max;
        if (ByteArrayComparer.Instance.Compare(max.Key, prefix) < 0)
        {
            yield break;
        }

        // GetViewBetween принимает пробы, которых нет в наборе, поэтому нижняя граница это просто prefix.
        // Подставлять сюда min нельзя: если prefix меньше всех ключей, а min уже больше successor,
        // GetViewBetween бросит ArgumentException.
        // prefix строго меньше successor, а при successor == null верхняя граница max не меньше prefix
        // (проверено выше), так что lower <= upper выполняется всегда.
        var lowerProbe = new KeyGroup<TModel>(prefix);

        byte[] successor = IncrementPrefix(prefix);
        KeyGroup<TModel> upperProbe = successor != null ? new KeyGroup<TModel>(successor) : max;

        foreach (var group in _entries.GetViewBetween(lowerProbe, upperProbe))
        {
            // successor это открытая верхняя граница, в диапазон она не входит.
            if (successor != null && ByteArrayComparer.Instance.Compare(group.Key, successor) >= 0)
            {
                continue;
            }
            yield return group.Models;
        }
    }

    /// <summary>
    /// <para>Returns the smallest array that is greater than every array starting with the prefix.</para>
    /// <para>Returns null if there is no such array, which happens when every byte of the prefix is 0xFF.</para>
    /// </summary>
    /// <param name="prefix">Encoded beginning of the key.</param>
    /// <returns>Exclusive upper bound of the keys that start with the prefix, or null.</returns>
    private static byte[] IncrementPrefix(byte[] prefix)
    {
        var result = (byte[])prefix.Clone();
        for (int i = result.Length - 1; i >= 0; i--)
        {
            if (result[i] != 0xFF)
            {
                result[i]++;
                Array.Resize(ref result, i + 1);
                return result;
            }
        }
        // Префикс состоит из одних 0xFF, следующего значения не существует.
        return null;
    }

    // Скомпилированный делегат быстрее PropertyInfo.GetValue, а вызывается он на каждой вставке и удалении.
    /// <summary>
    /// Compiles a delegate that reads the property and boxes its value to object.
    /// </summary>
    /// <param name="property">Property of the model.</param>
    /// <returns>Delegate that returns the value of the property for a model.</returns>
    protected static Func<TModel, object> CompileGetter(PropertyInfo property)
    {
        ParameterExpression param = Expression.Parameter(typeof(TModel), "m");
        MemberExpression propertyAccess = Expression.Property(param, property);
        UnaryExpression convert = Expression.Convert(propertyAccess, typeof(object));
        Expression<Func<TModel, object>> lambda = Expression.Lambda<Func<TModel, object>>(convert, param);
        return lambda.Compile();
    }
}