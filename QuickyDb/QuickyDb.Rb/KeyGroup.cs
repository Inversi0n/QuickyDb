using System.Collections.Generic;

namespace QuickyDb.Rb;

public sealed class KeyGroup<TModel>
{
    public readonly List<TModel> Models;
    public readonly byte[] Key;

    public KeyGroup(byte[] key)
    {
        Key = key;
        Models = new List<TModel>();
    }
}