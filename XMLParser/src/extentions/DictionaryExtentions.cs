using XmlParser.src.xml;

namespace XmlParser.src.extentions
{
    internal static class DictionaryExtentions
    {
        public static void Add<TKey, TValue>(this Dictionary<TKey, TValue> dic, Pair<TKey, TValue> item)
            where TKey : notnull
            where TValue : notnull
            => dic.Add(item.Key!, item.Value!);
    }
}
