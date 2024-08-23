using QuickyTree.FileUtils.Models;

namespace QuickyTree.Interfaces
{
    public interface IFileWrapper<T> where T : new()
    {
        public string FilePath { get; }
        T Read(ModelUnitMetadata fileInfo);
        T[] Reads(ModelUnitMetadata[] fileInfos);
        ModelUnitMetadata Write(T data);
    }
}
