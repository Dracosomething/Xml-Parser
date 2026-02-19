using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xml_parser.src.xml
{
    /// <summary>
    /// Basic wrapper class for a kev value pair, just without the unessesarily long name.
    /// </summary>
    internal struct Pair<K, V>
    {
        private K key;
        private V value;
    
        public K Key
        {
            get { return key; }
        }

        public V Value
        {
            get { return value; }
        }

        public Pair(K key, V value) {
            this.key = key;
            this.value = value;
        }

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
    }
}
