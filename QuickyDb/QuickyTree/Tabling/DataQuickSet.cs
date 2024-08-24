using QuickyTree.Interfaces;
using QuickyTree.Tree;
using System;
using System.Linq.Expressions;
using QuickyTree.FileUtils;
using QuickyTree.Models.Attributes;
using QuickyTree.Models;

namespace QuickyTree.Tabling
{
    public class DataQuickSet<TModel> where TModel : IModel, new()
    {
        public StoringConfig Config { get; set; }
        public string Path { get; private set; }
        public string Name { get; }
        public DataQuickSet(string path = @"C:\DataDisk\Quicky\DefaultProject\")
        {
            Name = typeof(TModel).Name;
            Path = path;
            _tableInstance = new FileWrapper<TModel>(Name);

            var properties = typeof(TModel).GetProperties(System.Reflection.BindingFlags.Public);
            var indexProperties = properties.Where(p => p.GetCustomAttributes(typeof(IndexAttribute), true)?.Length > 0).ToArray(); ;
            var tempIndexes = new QTree[indexProperties.Length];
            for (int i = 0; i < indexProperties.Length; i++)
            {
                tempIndexes[i] = new QTree();
            }
            _indexes = tempIndexes;
        }
        private readonly IFileWrapper<TModel> _tableInstance;
        private QTree _id = new QTree();
        private QTree[] _indexes = new QTree[1] { new QTree() };

        private QTree[] GetIndexes(TModel model)
        {
            return _indexes;
        }
        public void Add(TModel model)
        {
            var foundIndexes = GetIndexes(model);
            var fileUnit = _tableInstance.Write(model);

            var node = _indexes[0].Add(model.Id, fileUnit);
            //foreach (var index in foundIndexes)
            //{
            //    var node = index.Add(model as IComparable, fileUnit);
            //}



        }
        
        public void Save()
        {
            foreach (var index in _indexes)
            {
                index.Balance();
            }
        }
        public List<TModel> Search(params int[] ids)
        {
            //Parsing expression operations


            //applying expressoins

            //combining results

            var foundIndexes = _indexes.Where(i => true).ToArray();
            foundIndexes.Select(i => i.Search(0));
            var nodes = _indexes[0].SearchAll(ids.Cast<IComparable>().ToArray());
            var storingDatas = nodes.Select(n => n.StoringData).ToArray();
            var res = _tableInstance.Reads(storingDatas);
            //return nodes.Select(r => r.va).ToList();
            return res.ToList(); ;
        }
        public IQueryable<QNode> Search(Expression<Func<TModel, bool>> expression)
        {
            //Parsing expression operations


            //applying expressoins

            //combining results

            var foundIndexes = _indexes.Where(i => true).ToArray();
            foundIndexes.Select(i => i.Search(0));


            return null;
        }
    }
}
