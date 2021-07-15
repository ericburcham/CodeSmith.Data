using System.Linq;

namespace CodeSmith.Data.Future
{
    public class LoadedFutureValue<T> : IFutureValue<T>
    {
        public LoadedFutureValue(T value, IQueryable query)
        {
            Value = value;
            Query = query;
        }

        public bool IsLoaded => true;

        public IQueryable Query { get; }

        public T Value { get; }

        public void LoadValue(object o)
        {
        }
    }
}
