using QuickyTree.Interfaces;

namespace QuickyTree.Models
{
    public class IntModel : IModel
    {
        public int Id { get; set; }
        IComparable IModel.Id { get => Id; set => Id =(int) value; }
    }
}
