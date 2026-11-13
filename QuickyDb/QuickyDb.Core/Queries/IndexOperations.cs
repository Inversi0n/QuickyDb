using System;
using System.Collections.Generic;
using System.Reflection;

namespace QuickyDb.Core.Queries;

public sealed class IndexOperations<TModel>
{
    public Func<IEnumerable<TModel>> All { get; set; }
    public Func<PropertyInfo, object, IReadOnlyList<TModel>> GetEqual { get; set; }
    public Func<PropertyInfo, object, bool, IEnumerable<IReadOnlyList<TModel>>> RangeUpTo { get; set; }
    public Func<PropertyInfo, object, bool, IEnumerable<IReadOnlyList<TModel>>> RangeFrom { get; set; }
    public Func<PropertyInfo, object, IEnumerable<IReadOnlyList<TModel>>> AllExcept { get; set; }

    public IndexOperations()
    {
        
    }
}
