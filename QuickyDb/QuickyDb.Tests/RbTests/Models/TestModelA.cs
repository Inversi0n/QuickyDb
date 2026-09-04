using QuickyDb.Core.Common.Attributes;

namespace QuickyDb.Tests.RbTests.Models;

internal class TestModelA
{
    [Index]
    public int V1 { get; set; }
    [Index]
    public string V2 { get; set; }
}
