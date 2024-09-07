using QuickyTree.Interfaces;
using QuickyTree.Tree;
using System;
using System.Linq.Expressions;
using QuickyTree.FileUtils;
using QuickyTree.Models.Attributes;
using QuickyTree.Models;
using System.Diagnostics;

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
            _fileWrapper = new FileWrapper<TModel>(Name); //TODO Add shared mapper

            var properties = typeof(TModel).GetProperties(System.Reflection.BindingFlags.Public);
            var indexProperties = properties.Where(p => p.GetCustomAttributes(typeof(IndexAttribute), true)?.Length > 0).ToArray(); ;
            var tempIndexes = new QTree[indexProperties.Length];
            for (int i = 0; i < indexProperties.Length; i++)
            {
                tempIndexes[i] = new QTree();
            }
            _indexes = tempIndexes;
        }
        private readonly IFileWrapper<TModel> _fileWrapper;
        private QTree _id = new QTree();
        private QTree[] _indexes = new QTree[1] { new QTree() };

        private QTree[] GetIndexes(TModel model)
        {
            return _indexes;
        }
        public void Add(TModel model)
        {
            var foundIndexes = GetIndexes(model);
            var fileUnit = _fileWrapper.Write(model);

            var node = _indexes[0].Add(model.Id, fileUnit);
            //foreach (var index in foundIndexes)
            //{
            //    var node = index.Add(model as IComparable, fileUnit);
            //}



        }
        public void Remove(TModel model)
        {
            var node = this._indexes[0].Remove(model.Id);

            //Set to filewrapper that node.StoringData is removed
            //_fileWrapper.Remove(model.Id); //
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
            var res = _fileWrapper.Reads(storingDatas);
            //return nodes.Select(r => r.va).ToList();
            return res.ToList(); ;
        }
        public IQueryable<QNode> Search(Expression<Func<TModel, bool>> expression)
        {
            //Selection = "1";
            //Example<QNode, bool>(p => p.Value == Selection);

            //var body = expression.Body;
            ////var rightBody = expression
            //var isNullOrWhiteSpaceMethod = typeof(string).GetMethod(nameof(string.IsNullOrWhiteSpace));

            Example(expression);
            return null;
        }
        public double Selection { get; set; }

        public void Example<T, TResult>(Expression<Func<T, TResult>> exp)
        {
            if (exp.Body is BinaryExpression equality)
            {
                //BinaryExpression equality = (BinaryExpression)exp.Body;
                var nodeType = equality.NodeType;
                var method = equality.Method;

                if (equality.Left != null)
                {
                    
                }

                Debug.Assert(equality.NodeType == ExpressionType.GreaterThanOrEqual);

                // Note that you need to know the type of the rhs of the equality
                var rightAccessorExpression = Expression.Lambda<Func<double>>(equality.Right);
                var leftAccessorExpression = Expression.Lambda<Func<double>>(equality.Left);
                Func<double> rightAccessor = rightAccessorExpression.Compile();
                Func<double> leftAccessor = leftAccessorExpression.Compile();
                var rightValue = rightAccessor();
                var leftValue = leftAccessor();
                
                Debug.Assert(rightValue == Selection);
            }
            else
            {
                var a = 0;
            }
        }
        enum ExpressionAction
        {
            Compare,
            StringContains,
            ContainsInArray,


        }
    }
}
