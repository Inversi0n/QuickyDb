using QuickyTree.FileUtils.Models;
using QuickyTree.Interfaces;
using System.IO;
using System.Text;

namespace QuickyTree.FileUtils
{
    public class FileWrapper : IFileWrapper
    {
        public string FilePath { get; private set; }

        public FileWrapper(string name)
        {

            FilePath = $"C:\\DataDisk\\Quicky\\{name}";
        }
        private object _lock = new object();
        public string Read(ModelUnitMetadata fileInfo)
        {
            using var stream = GetRead();

            var buffer = new byte[fileInfo.Length];
            var len = stream.Read(buffer, (int) fileInfo.From, fileInfo.Length);

            return Encoding.UTF8.GetString(buffer);
        }

        private FileStream GetRead()
        {
            return new FileStream(FilePath, new FileStreamOptions()
            {
                BufferSize = 4096 / 2,
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.Read,
            });
        }
        private FileStream GetWrite()
        {
            return new FileStream(FilePath, new FileStreamOptions()
            {
                BufferSize = 4096 / 2,
                Access = FileAccess.Write,
                Mode = FileMode.OpenOrCreate,
                Options = FileOptions.SequentialScan,
                Share = FileShare.None,
            });
        }
        public string[] Reads(ModelUnitMetadata[] fileInfos)
        {
            throw new NotImplementedException();
        }

        public ModelUnitMetadata Write(string data)
        {
            using var stream = GetWrite();
            var buffer = Encoding.UTF8.GetBytes(data);
            stream.Write(buffer);

            var fileLen = stream.Length;

            return new ModelUnitMetadata(FilePath, fileLen, buffer.Length);
        }
    }
}
