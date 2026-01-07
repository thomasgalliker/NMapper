using System.Collections;

namespace NMapper.Internals
{
    public interface ICollectionFactory
    {
        Array CreateArray(Type elementType, int length);

        IList CreateList(Type elementType);
    }
}
