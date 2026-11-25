using QuickyDb.Core.Common.Attributes;

namespace QuickyDb.Tests.RbTests.Models;

internal class Person
{
    [CompositeIndex("Fio", 0)]
    public string FirstName { get; set; }
    [CompositeIndex("Fio", 1)]
    public string LastName { get; set; }
    public string Email { get; set; }
}
