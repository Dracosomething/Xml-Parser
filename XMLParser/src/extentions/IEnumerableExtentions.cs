using System.Text;
using XmlParser.src.extentions.@string;

namespace XmlParser.src.extentions
{
    internal static class IEnumerableExtentions
    {
        public static string AsString<T>(this IEnumerable<T> self)
        {
            var builder = new StringBuilder("[");
            foreach (var item in self)
                builder.Append(item == null ? "null" : item).Append(", ");
            return builder.Append("]").ToString().RemoveLast(", ");
        }
    }
}
