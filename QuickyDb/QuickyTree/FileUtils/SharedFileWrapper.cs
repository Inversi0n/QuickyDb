using QuickyTree.FileUtils.Models;
using QuickyTree.Interfaces;
using System.IO;

namespace QuickyTree.FileUtils
{
    public class SharedFileWrapper<T> : IFileWrapper<T>
        where T : new()
    {
        private List<FileWrapper<T>> _wrappers;
        public SharedFileWrapper(List<FileWrapper<T>> baseFiles)
        {
            _wrappers = baseFiles;
        }//TODO Need to make a factory or loader. To create file wrappers[] from out filename(or path with name)
        public string FilePath => throw new NotImplementedException();

        public T Read(ModelUnitMetadata fileInfo)
        {
            if (fileInfo.Page < 0 || fileInfo.Page >= _wrappers.Count)
            {
                var exStr = $"Bad page in model. requesed {fileInfo.Page}. But length is {_wrappers.Count}";
                throw new Exception(exStr);
            }
            var wrapper = _wrappers[fileInfo.Page];
            var res = wrapper.Read(fileInfo);

            return res;
        }

        public T[] Reads(ModelUnitMetadata[] fileInfos)
        {
            if (fileInfos.Any(f => f.Page < 0 || f.Page >= _wrappers.Count))
            {
                //requesed {fileInfo.Page}
                var exStr = $"Bad page in model. . But length is {_wrappers.Count}";
                throw new Exception(exStr);
            }
            //group
            var stack = new Stack<int>(Enumerable.Range(0, 10_000));
            var groupping = fileInfos.Select(o => new { o, index = stack.Pop() }).GroupBy(f => f.o.Page).ToArray();

            var result = new T[fileInfos.Length];
            foreach (var group in groupping)
            {
                var grWrappers = group.ToArray();
                var unitsInpage = grWrappers.Select(w => w.o).ToArray();
                var indexes = grWrappers.Select(w => w.index).ToArray();

                var page = group.Key;
                var wrResult = _wrappers[page].Reads(unitsInpage);
                for (var i = 0; i < wrResult.Length; i++)
                {
                    result[indexes[i]] = wrResult[i];
                }
            }
            return result;
        }

        public ModelUnitMetadata Write(T data)
        {
            var wrapper = _wrappers.Last();

            if (true)
            {
                _wrappers.Add(new FileWrapper<T>(FilePath));
                wrapper = _wrappers.Last();
            }

            var res = wrapper.Write(data);

            return res;
        }
    }
}
