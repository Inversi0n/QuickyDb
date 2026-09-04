using QuickyDb.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace QuickyDb.Rb.IndexedStore
{
    internal abstract class IndexedStoreBase<TModel>
    {
        protected readonly SortedSet<KeyGroup<TModel>> _entries;
        protected Func<TModel, byte[]> GetKeys;

        public IndexedStoreBase()
        {
            var comparer = Comparer<KeyGroup<TModel>>.Create(
                (a, b) => ByteArrayComparer.Instance.Compare(a.Key, b.Key));
            _entries = new SortedSet<KeyGroup<TModel>>(comparer);
        }


        public void Add(TModel model)
        {
            var key = GetKeys(model);
            var probe = new KeyGroup<TModel>(key);

        if (!_entries.TryGetValue(probe, out var group))
        {
            group = probe;
            _entries.Add(group);
        }
        group.Models.Add(model);
    }

        public void Remove(TModel model)
        {
            var key = GetKeys(model);
            var probe = new KeyGroup<TModel>(key);
            if (!_entries.TryGetValue(probe, out var group)) return;

        group.Models.Remove(model);
        if (group.Models.Count == 0)
            _entries.Remove(group);
    }

    public bool TryGetEqual(byte[] key, out List<TModel> models)
    {
        var probe = new KeyGroup<TModel>(key);
        if (_entries.TryGetValue(probe, out var group))
        {
            models = group.Models;
            return true;
        }
        models = null;
        return false;
    }

    public IEnumerable<List<TModel>> RangeUpTo(byte[] bound, bool inclusive)
    {
        if (_entries.Count == 0) yield break;

        var min = _entries.Min;
        if (ByteArrayComparer.Instance.Compare(min.Key, bound) > 0) yield break;

        var probe = new KeyGroup<TModel>(bound);
        foreach (var group in _entries.GetViewBetween(min, probe))
        {
            if (!inclusive && ByteArrayComparer.Instance.Equals(group.Key, bound)) continue;
            yield return group.Models;
        }
    }

    public IEnumerable<List<TModel>> RangeFrom(byte[] bound, bool inclusive)
    {
        if (_entries.Count == 0) yield break;

        var max = _entries.Max;
        if (ByteArrayComparer.Instance.Compare(max.Key, bound) < 0) yield break;

        var probe = new KeyGroup<TModel>(bound);
        foreach (var group in _entries.GetViewBetween(probe, max))
        {
            if (!inclusive && ByteArrayComparer.Instance.Equals(group.Key, bound)) continue;
            yield return group.Models;
        }
    }

    public IEnumerable<List<TModel>> AllExcept(byte[] excludedKey)
    {
        foreach (var group in _entries)
        {
            if (ByteArrayComparer.Instance.Equals(group.Key, excludedKey)) continue;
            yield return group.Models;
        }
    }

      
        // Template for comaring compount indexes      
        public IEnumerable<List<TModel>> PrefixScan(byte[] prefix)
        {
            if (_entries.Count == 0 || prefix.Length == 0) yield break;

            var min = _entries.Min;
            var max = _entries.Max;
            if (ByteArrayComparer.Instance.Compare(max.Key, prefix) < 0) yield break;

            var lowerProbe = new KeyGroup<TModel>(prefix);
            var from = ByteArrayComparer.Instance.Compare(lowerProbe.Key, min.Key) < 0 ? min : lowerProbe;

            var successor = IncrementPrefix(prefix);
            var upperProbe = successor != null ? new KeyGroup<TModel>(successor) : max;

            foreach (var group in _entries.GetViewBetween(from, upperProbe))
            {
                if (successor != null && ByteArrayComparer.Instance.Compare(group.Key, successor) >= 0)
                    continue; // successor — открытая верхняя граница, не часть диапазона
                yield return group.Models;
            }
        }

        private byte[] IncrementPrefix(byte[] prefix)
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
            return null; // префикс — все 0xFF, конечного "следующего" значения не существует
        }

        protected static Func<TModel, object> CompileGetter(PropertyInfo property)
        {
            var param = Expression.Parameter(typeof(TModel), "m");
            var propertyAccess = Expression.Property(param, property);
            var convert = Expression.Convert(propertyAccess, typeof(object));
            var lambda = Expression.Lambda<Func<TModel, object>>(convert, param);
            return lambda.Compile();
        }
    }
}
