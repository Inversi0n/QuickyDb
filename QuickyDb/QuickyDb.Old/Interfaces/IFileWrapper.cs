using QuickyDb.Old.FileUtils.Models;

namespace QuickyDb.Old.Interfaces;

public interface IFileWrapper<T> where T : new()
{
    public string FilePath { get; }
    T Read(SavedLocationMetadata fileInfo);
    T[] Reads(SavedLocationMetadata[] fileInfos);
    SavedLocationMetadata Write(T data);
}
