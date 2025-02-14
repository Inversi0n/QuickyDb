using QuickyTree.FileUtils.Models;

namespace QuickyTree.Interfaces;

public interface IFileWrapper<T> where T : new()
{
    public string FilePath { get; }
    T Read(SavedLocationMetadata fileInfo);
    T[] Reads(SavedLocationMetadata[] fileInfos);
    SavedLocationMetadata Write(T data);
}
