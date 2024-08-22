using QuickyTree.Interfaces;
using QuickyTree.Tree;
using System;
using System.Linq.Expressions;

namespace QuickyTree
{
    public class DataQuickSet<TModel>
    {

        public DataQuickSet()
        {

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
        IQueryable<QTree> Search(Expression<Func<TModel, bool>> expression)
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
