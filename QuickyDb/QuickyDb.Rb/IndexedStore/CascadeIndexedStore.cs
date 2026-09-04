using QuickyDb.Core.Common;
using System;
using System.Reflection;

namespace QuickyDb.Rb.IndexedStore;

internal sealed class CascadeIndexedStore<TModel> : IndexedStoreBase<TModel>
{
    public PropertyInfo[] Properties { get; }

    private readonly Func<TModel, object>[] _getValues;

    public CascadeIndexedStore(PropertyInfo[] properties) : base()
    {
        Properties = properties;
        _getValues = new Func<TModel, object>[properties.Length];
        for (int i = 0; i < properties.Length; i++)
        {
            PropertyInfo prop = properties[i];
            _getValues[i] = CompileGetter(prop);
        }

        GetKeys = model =>
        {
            var parts = new byte[_getValues.Length][];
            int totalLength = 0;
            for (int i = 0; i < _getValues.Length; i++)
            {
                var value = _getValues[i](model);
                bool isLast = i == _getValues.Length - 1;
                parts[i] = ByteEncoder.EncodeComponent(value, isLast);
                totalLength += parts[i].Length;
            }

            var result = new byte[totalLength];
            int offset = 0;
            foreach (var part in parts)
            {
                Buffer.BlockCopy(part, 0, result, offset, part.Length);
                offset += part.Length;
            }
            return result;
        };
    }
}
