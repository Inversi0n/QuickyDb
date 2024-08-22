using QuickyTree.Interfaces;
using QuickyTree.Tree;
using System;
using System.Linq.Expressions;
using QuickyTree.FileUtils.Models;
using QuickyTree.FileUtils;

namespace QuickyTree
{
    public class DataQuickSet<TModel>
    {
        public string Name { get; }
        public DataQuickSet()
        {
            Name = typeof(TModel).Name;
            _tableInstance = new FileWrapper(Name);
        }
        private readonly IFileWrapper _tableInstance;
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
                var fileUnit = _tableInstance.Write(model.ToString());
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
        public List<QNode> Search(params int[] ids)
        {
            //Parsing expression operations


            //applying expressoins

            //combining results

            var foundIndexes = _indexes.Where(i => true).ToArray();
            foundIndexes.Select(i => i.Search(0));
            var res = this._indexes[0].SearchAll(ids.Cast<IComparable>().ToArray());
            return res.ToList();
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
