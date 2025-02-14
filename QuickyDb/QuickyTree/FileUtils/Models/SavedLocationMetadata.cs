namespace QuickyTree.FileUtils.Models;

public class SavedLocationMetadata
{
    public string FileName { get; set; }
    public int Page { get; set; }
    public long From { get; set; }
    public int Length { get; set; }
    public SavedLocationMetadata(string fileName, long from, int length)
    {
        FileName = fileName;
        From = from;
        Length = length;
    }
    public SavedLocationMetadata()
    {

    }
}
