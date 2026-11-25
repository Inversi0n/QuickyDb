using System;
using System.Collections.Generic;
using System.Reflection;

namespace QuickyDb.Core.Queries;

/// <summary>
/// <para>Operations of a table that <see cref="QueryExecutor{TModel}"/> uses to find models.</para>
/// <para>Every delegate must be assigned before the executor is used.</para>
/// </summary>
/// <typeparam name="TModel">Type of the model.</typeparam>
public sealed class IndexOperations<TModel>
{
    /// <summary>
    /// Returns all models of the table.
    /// </summary>
    public Func<IEnumerable<TModel>> All { get; set; }

    /// <summary>
    /// <para>Returns the models whose property is equal to the value.</para>
    /// <para>Returns null or an empty list if there are no such models.</para>
    /// </summary>
    public Func<PropertyInfo, object, IReadOnlyList<TModel>> GetEqual { get; set; }

    /// <summary>
    /// <para>Returns the models whose property is less than the bound, in groups of equal values.</para>
    /// <para>The third argument tells whether the models equal to the bound are included.</para>
    /// </summary>
    public Func<PropertyInfo, object, bool, IEnumerable<IReadOnlyList<TModel>>> RangeUpTo { get; set; }

    /// <summary>
    /// <para>Returns the models whose property is greater than the bound, in groups of equal values.</para>
    /// <para>The third argument tells whether the models equal to the bound are included.</para>
    /// </summary>
    public Func<PropertyInfo, object, bool, IEnumerable<IReadOnlyList<TModel>>> RangeFrom { get; set; }

    /// <summary>
    /// Returns the models whose property is not equal to the value, in groups of equal values.
    /// </summary>
    public Func<PropertyInfo, object, IEnumerable<IReadOnlyList<TModel>>> AllExcept { get; set; }

    /// <summary>
    /// Creates a set of operations with unassigned delegates.
    /// </summary>
    public IndexOperations()
    {
    }
}
