using QuickyDb.Old.Interfaces;
using System;

namespace QuickyDb.Old.Models;

public class GuidModel:IModel
{
    public Guid Id { get; set; }
    IComparable IModel.Id { get => Id; set => Id = (Guid)value; }
}
