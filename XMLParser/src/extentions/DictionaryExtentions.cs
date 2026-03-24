using System.Text;
using XmlParser.src.extentions.@string;
using XmlParser.src.xml;

namespace XmlParser.src.extentions
{
    internal static class DictionaryExtentions
    {
        public static void Add<TKey, TValue>(this Dictionary<TKey, TValue> dic, Pair<TKey, TValue> item)
            where TKey : notnull
            where TValue : notnull
            => dic.Add(item.Key!, item.Value!);

        public static string AsString<TKey, TValue>(this Dictionary<TKey, TValue> self)
            where TKey : notnull
            where TValue : notnull
        {
            var builder = new StringBuilder("[");
            foreach (var item in self)
                builder.Append(((Pair<TKey, TValue>)item).ToString()).Append(", ");
            return builder.Append("]").ToString().RemoveLast(", ");
        }
    }
}
