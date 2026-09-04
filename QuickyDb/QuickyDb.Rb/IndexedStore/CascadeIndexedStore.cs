using QuickyDb.Core.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace QuickyDb.Rb.IndexedStore
{
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
                var span = new Span<byte>();
                for (int i = 0; i < _getValues.Length; i++)
                {
                    var value = _getValues[i](model);
                    var key = ByteEncoder.Encode(value);
                    foreach (var b in key)
                    {
                        if (b == 0x00)
                            span.Fill(0xFF); //TODO Define length by this element
                        else
                            span.Fill(b);
                    }
                    if (i + 1 < _getValues.Length)
                        span.Fill(0x00);
                }
                return span.ToArray();
            };
        }
    }
}
