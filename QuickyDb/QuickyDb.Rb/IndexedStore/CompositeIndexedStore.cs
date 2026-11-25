using System;
using System.Reflection;
using QuickyDb.Core.Common;

namespace QuickyDb.Rb.IndexedStore;

/// <summary>
/// <para>Index over several properties at once.</para>
/// <para>The key is the concatenation of the encoded values in the order of the properties.</para>
/// </summary>
/// <typeparam name="TModel">Type of the model.</typeparam>
internal sealed class CompositeIndexedStore<TModel> : IndexedStoreBase<TModel>
{
    public PropertyInfo[] Properties { get; }

    private readonly Func<TModel, object>[] _getValues;

    public CompositeIndexedStore(PropertyInfo[] properties) : base()
    {
        Properties = properties;
        _getValues = new Func<TModel, object>[properties.Length];
        for (int i = 0; i < properties.Length; i++)
        {
            PropertyInfo prop = properties[i];
            _getValues[i] = CompileGetter(prop);
        }

        // Строки всех компонентов, кроме последнего, получают терминатор: иначе границы компонентов
        // в склеенном ключе не различить (ByteEncoder.EncodeComponent).
        GetKeys = model =>
        {
            var parts = new byte[_getValues.Length][];
            int totalLength = 0;
            for (int i = 0; i < _getValues.Length; i++)
            {
                object value = _getValues[i](model);
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
