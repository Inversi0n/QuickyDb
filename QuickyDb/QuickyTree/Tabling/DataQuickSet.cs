using QuickyTree.Interfaces;
using QuickyTree.Tree;
using System;
using System.Linq.Expressions;
using QuickyTree.FileUtils;
using System.Security.Claims;

namespace QuickyTree.Tabling
{
    public class DataQuickSet<TModel> where TModel : IModel, new()
    {
        public string Name { get; }
        public DataQuickSet()
        {
            Name = typeof(TModel).Name;
            _tableInstance = new FileWrapper<TModel>(Name);
        }
        private readonly IFileWrapper<TModel> _tableInstance;
        private QTree[] _indexes = new QTree[1] { new QTree() };

        private QTree[] GetIndexes(TModel model)
        {
            return _indexes;
        }
        public void Add(TModel model)
        {
            var foundIndexes = GetIndexes(model);
            foreach (var index in foundIndexes)
            {
                var fileUnit = _tableInstance.Write(model);
                var node = index.Add(model as IComparable, fileUnit);
            }

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
            return null;
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
