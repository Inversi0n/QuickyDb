using System.Linq.Expressions;

namespace QuickyTree.SqlParsing.SqlToExpression
{
    public class Converter
    {
        public Expression[] Foo(string sql)
        {
            var lines = sql.Split(new char[] { '\n' }).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            Expression curEx = null;
            foreach (var line in lines)
            {
                var words = line.Split(new char[] { ',', ' ', '.' }).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                foreach (var word in words)
                {
                    if (curEx == null)
                    {
                        if (word.ToLower() != "select")
                            throw new Exception($"Expected Select, not {word}");

                    }
                }
            }
            return default;
        }
        private
    }
}
