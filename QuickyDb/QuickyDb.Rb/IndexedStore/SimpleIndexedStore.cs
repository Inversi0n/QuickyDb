using System;
using System.Reflection;
using QuickyDb.Core.Common;

namespace QuickyDb.Rb.IndexedStore;

/// <summary>
/// Index over one property. The key is the encoded value of the property.
/// </summary>
/// <typeparam name="TModel">Type of the model.</typeparam>
internal sealed class SimpleIndexedStore<TModel> : IndexedStoreBase<TModel>
{
    public PropertyInfo Property { get; }

    private readonly Func<TModel, object> _getValue;

    public SimpleIndexedStore(PropertyInfo property) : base()
    {
        Property = property;
        _getValue = CompileGetter(property);

        GetKeys = model => ByteEncoder.Encode(_getValue(model));
    }
}