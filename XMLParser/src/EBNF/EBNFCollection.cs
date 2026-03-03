using System;
using System.Collections.Generic;
using System.Text;

namespace XmlParser.src.EBNF
{
    internal class EBNFCollection
    {
        private Dictionary<int, int> characters = new Dictionary<int, int>();
        private Dictionary<int, Range> ranges = new Dictionary<int, Range>();
        private int index = 0;
        private bool not = false;
        
        public bool Not
        {
            get => not;
            set
            {
                if (not) return;
                not = value;
            }
        }

        public bool Add(char value) => characters.TryAdd(index++, value);

        public bool Add(int value) => characters.TryAdd(index++, value);

        public bool Add(Range value) => ranges.TryAdd(index++, value);
    
        public bool TryGet(int index, out char value)
        {
            if (characters.ContainsKey(index))
            {
                value = (char)characters[index];
                return true;
            }

            value = '\0';
            return false;
        }

        public bool TryGet(int index, out int value)
        {
            if (characters.ContainsKey(index))
            {
                value = characters[index];
                return true;
            }

            value = '\0';
            return false;
        }

        public bool TryGet(int index, out Range value)
        {
            if (ranges.ContainsKey(index))
            {
                value = ranges[index];
                return true;
            }

            value = default!;
            return false;
        }

        public bool Check(char c)
        {
            if (characters.Count != 0 && characters.ContainsValue(c))
                return true;
            if (ranges.Count != 0)
            {
                foreach(var pair in ranges)
                {
                    var range = pair.Value;
                    if (range.IsBetween(c))
                        return true;
                }
            }
            return false;
        }
    
        public string ToString()
        {
            StringBuilder builder = new();
            builder.Append("index: ").Append(index).AppendLine();
            builder.Append("inverse: ").Append(not).AppendLine();
            for (int i = 0; i < index; i++)
            {
                builder.Append("{ ");
                builder.Append("[").Append(i).Append("] => ");
                if (characters.ContainsKey(i))
                    builder.Append(characters[i]).Append("/").Append((char)characters[i]);
                else
                    builder.Append(ranges[i]);
                builder.Append(" }, ");
            }
            return builder.ToString();
        }
    }
}
