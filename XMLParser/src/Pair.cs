using System.Text;

namespace XmlParser.src.xml
{
    /// <summary>
    /// Basic wrapper class for a kev value pair, just without the unessesarily long name.
    /// </summary>
    internal class Pair<K, V>
    {
        public K? Key { get; init; }

        public V? Value { get; init; }

        public override string ToString() =>
            new StringBuilder("[")
            // if Key is null append a string literal "null" otherwise turn Key into a string.
            .Append(Key == null ? "null" : Key.ToString()).Append(", ")
            .Append(Value == null ? "null" : Value.ToString()).Append("]")
            .ToString();

        public static implicit operator Pair<K, V>(KeyValuePair<K, V> pair) => new Pair<K, V> { Key = pair.Key, Value = pair.Value };
        public static implicit operator KeyValuePair<K, V>(Pair<K, V> pair) => new KeyValuePair<K, V>(pair.Key, pair.Value!);
    }
}
