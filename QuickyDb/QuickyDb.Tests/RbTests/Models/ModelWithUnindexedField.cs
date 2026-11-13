using QuickyDb.Core.Common.Attributes;

namespace QuickyDb.Tests.RbTests.Models;

internal class ModelWithUnindexedField
{
    [Index]
    public int Id { get; set; }

    // Намеренно без [Index] — используется, чтобы проверить поведение
    // RbTable.Search при обращении к непроиндексированному свойству.
    public string Description { get; set; } = "";
}
