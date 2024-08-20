using QuickyTree.FileUtils.Models;

namespace QuickyTree.Interfaces
{
    public interface IFileWrapper
    {
        public string FileName { get; }
        string Read(ModelUnitMetadata fileInfo);
        string[] Reads(ModelUnitMetadata fileInfos);
        ModelUnitMetadata Write(string data);
    }
}
