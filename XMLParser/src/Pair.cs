using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlParser.src.xml
{
    /// <summary>
    /// Basic wrapper class for a kev value pair, just without the unessesarily long name.
    /// </summary>
    internal struct Pair<K, V>
    {
        public K Key { get; init; }

        public V Value { get; init; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");

            // if Key is null append a string literal "null" otherwise turn Key into a string.
            builder.Append(Key == null ? "null" : Key.ToString());
            builder.Append(",");
            builder.Append(Value == null ? "null" : Value.ToString());

            builder.Append("]");
            return base.ToString();
        }

        public static explicit operator Pair<K, V>(KeyValuePair<K, V> pair) => new Pair<K, V> { Key = pair.Key, Value = pair.Value };

        public static implicit operator KeyValuePair<K, V>(Pair<K, V> pair) => new KeyValuePair<K, V>(pair.Key, pair.Value);
    }
}
