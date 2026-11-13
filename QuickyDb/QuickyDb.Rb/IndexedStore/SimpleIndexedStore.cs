using QuickyDb.Core.Common;
using System;
using System.Reflection;

namespace QuickyDb.Rb.IndexedStore;

internal sealed class SimpleIndexedStore<TModel>: IndexedStoreBase<TModel>
{
    public PropertyInfo Property { get; }

    private readonly Func<TModel, object> _getValue;

    public SimpleIndexedStore(PropertyInfo property):base()
    {
        Property = property;
        _getValue = CompileGetter(property);

        GetKeys = model => ByteEncoder.Encode(_getValue(model));
    }       
}