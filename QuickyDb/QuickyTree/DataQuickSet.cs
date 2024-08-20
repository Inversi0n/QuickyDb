using QuickyTree.Interfaces;
using QuickyTree.Tree;
using System.Linq.Expressions;

namespace QuickyTree
{
    public class DataQuickSet<TModel>
    {

        public DataQuickSet()
        {

        }
        private readonly IFileWrapper _tableInstance;
        private QTree[] _indexes;


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
