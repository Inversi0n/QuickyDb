namespace QuickyTree.Models
{
    public class StoringConfig
    {
        public long MaxFileSize { get; set; } = 1024 * 1024;
        public long CurrentSize { get; set; }
        public long MaxDeadDize { get; set; }
        public string[] Splits { get; set; }
    }
}
