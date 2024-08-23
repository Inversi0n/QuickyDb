using QuickyTree.Interfaces;

namespace QuickyTree.Models
{
    public class GuidModel:IModel
    {
        public Guid Id { get; set; }
        IComparable IModel.Id { get => Id; set => Id = (Guid)value; }
    }
}
