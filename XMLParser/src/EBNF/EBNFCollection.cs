using System;
using System.Collections.Generic;
using System.Text;

namespace XMLParser.src.EBNF
{
    internal class EBNFCollection
    {
        private Dictionary<int, int> characters = new Dictionary<int, int>();
        private Dictionary<int, Range> ranges = new Dictionary<int, Range>();
        private int index = 0;
        
        public bool Not
        {
            get;
            set
            {
                if (Not) return;
                Not = value;
            }
        }

        public bool Add(char value) => characters.TryAdd(++index, value);

        public bool Add(int value) => characters.TryAdd(++index, value);

        public bool Add(Range value) => ranges.TryAdd(++index, value);
    
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
    }
}
