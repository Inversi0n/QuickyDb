namespace QuickyTree.FileUtils.Models
{
    public class ModelUnitMetadata
    {
        public string FileName { get; set; }
        public long From { get; set; }
        public int Length { get; set; }
        public ModelUnitMetadata(string fileName, long from, int length)
        {
            FileName = fileName;
            From = from;
            Length = length;
        }
        public ModelUnitMetadata()
        {

        }
    }
}
