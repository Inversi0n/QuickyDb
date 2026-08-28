using QuickyDb.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace QuickyDb.Rb
{
    internal sealed class IndexedStore<TModel>
    {
        public PropertyInfo Property { get; }

        private readonly Func<TModel, object> _getValue;
        private readonly SortedSet<KeyGroup<TModel>> _entries;

        public IndexedStore(PropertyInfo property)
        {
            Property = property;
            _getValue = CompileGetter(property);

            var comparer = Comparer<KeyGroup<TModel>>.Create(
                (a, b) => ByteArrayComparer.Instance.Compare(a.Key, b.Key));
            _entries = new SortedSet<KeyGroup<TModel>>(comparer);
        }

        public void Add(TModel model)
        {
            var key = ByteEncoder.Encode(_getValue(model));
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
            var key = ByteEncoder.Encode(_getValue(model));
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

        private static Func<TModel, object> CompileGetter(PropertyInfo property)
        {
            var param = Expression.Parameter(typeof(TModel), "m");
            var propertyAccess = Expression.Property(param, property);
            var convert = Expression.Convert(propertyAccess, typeof(object));
            var lambda = Expression.Lambda<Func<TModel, object>>(convert, param);
            return lambda.Compile();
        }
    }
}