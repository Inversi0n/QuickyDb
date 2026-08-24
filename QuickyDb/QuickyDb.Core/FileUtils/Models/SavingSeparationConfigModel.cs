using System;

namespace QuickyTree.FileUtils.Models
{
    public class SavingSeparationConfigModel
    {
        public long MaxFileLength { get; set; }
        public Guid CurId { get; set; }
        public string Path { get; set; }
        public SavingSeparationConfigModel(long maxFileLength, Guid curId, string path)
        {
            MaxFileLength = maxFileLength;
            CurId = curId;
            Path = path;
        }
        public SavingSeparationConfigModel()
        {

        }
    }
}
