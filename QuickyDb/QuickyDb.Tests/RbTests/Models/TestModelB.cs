using QuickyDb.Core.Common.Attributes;

namespace QuickyDb.Tests.RbTests.Models;

internal class TestModelB
{
    [Index]
    public int V1 { get; set; }
    [CascadeIndex("in", 1)]
    public string V2 { get; set; }
    [CascadeIndex("in", 0)]
    public byte V3 { get; set; }
}
