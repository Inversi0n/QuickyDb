using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using QuickyTree.FileUtils.Models;
using QuickyTree.Interfaces;
using System.IO;

namespace QuickyTree.FileUtils
{
    public class FileWrapper<T> : IFileWrapper<T> where T : new()
    {
        public string FilePath { get; private set; }

        public FileWrapper(string filePath)
        {
            FilePath = filePath;
        }
        private readonly object _lock = new object();

        public ModelUnitMetadata Write(T data)
        {
            using var stream = GetWrite();
            var buffer = ToBson(data);//Encoding.UTF8.GetBytes(data);

            var position = stream.Length;
            lock (_lock)
            {
                stream.Write(buffer);
            }
            var fileLen = stream.Length;

            return new ModelUnitMetadata(FilePath, position/*fileLen - buffer.Length*/, buffer.Length);
        }
        public T Read(ModelUnitMetadata fileInfo)
        {
            using var stream = GetRead();

            var buffer = new byte[fileInfo.Length];
            var len = stream.Read(buffer, (int)fileInfo.From, fileInfo.Length);
            if (len < buffer.Length)
            {
                var bufT = new byte[len];
                Array.Copy(buffer, bufT, len);
                buffer = bufT;
            }
            var res = FromBson(buffer);
            return res;
        }


        public T[] Reads(ModelUnitMetadata[] fileInfos)
        {
            using var stream = GetRead();

            var orderedfileInfos = fileInfos.OrderBy(fi1 => fi1.From).ToArray(); ;

            var results = new List<T>(fileInfos.Length);
            for (int i = 0; i < orderedfileInfos.Length; i++)
            {
                ModelUnitMetadata fileInfo = orderedfileInfos[i];

                var j = 1;
                var totalLength = fileInfo.Length;
                while (i + j < orderedfileInfos.Length)
                {
                    var fi = orderedfileInfos[i + j];

                    if (fileInfo.From + fileInfo.Length == fi.From)
                    {
                        totalLength += fi.Length;
                    }
                    j++;
                }

                var buffer = new byte[totalLength];
                stream.Position = fileInfo.From;
                var len = stream.Read(buffer, 0, totalLength);
                if (len < buffer.Length)
                {
                    var bufT = new byte[len];
                    Array.Copy(buffer, bufT, len);
                    buffer = bufT;
                }
                for (int k = 0; k < j; k++)
                {
                    var fi = orderedfileInfos[i + k];
                    var buf = new byte[fi.Length];
                    Array.Copy(buffer, fi.From - fileInfo.From, buf, 0, buf.Length);
                    var res = FromBson(buf);
                    results.Add(res);
                }
                j--;
                i += j;

            }
            return results.ToArray();
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
                Mode = FileMode.Append,
                Options = FileOptions.SequentialScan,
                Share = FileShare.None,
            });
        }

        public static byte[] ToBson(T value)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BsonDataWriter datawriter = new BsonDataWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(datawriter, value);
                return ms.ToArray();
            }
        }

        public static T FromBson(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BsonDataReader reader = new BsonDataReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }
    }
}
