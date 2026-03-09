using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using XmlParser.src.xml;

namespace XmlParser.src
{
    internal class LookupTable<TKey, TValue>
    {
        // implementation needed
        // requires a dictionary and only an add method
        // requires a read only dictionary
        private ReadOnlyDictionary<TKey, TValue> table;
        public int Capacity { get; init; } = -1;
        private bool initialized = false;

        public TValue this[TKey key] => Get(key);

        public LookupTable() { }

        public LookupTable(int capacity) { Capacity = capacity; }

        public LookupTable(Dictionary<TKey, TValue> dic)
        {
            table = new ReadOnlyDictionary<TKey, TValue>(dic);
            Capacity = dic.Capacity;
            initialized = true;
        }

        public LookupTable<TKey, TValue> Fill(Func<Dictionary<TKey, TValue>, Dictionary<TKey, TValue>> func)
        {
            if (initialized)
                throw new FieldAccessException();
            if (Capacity == -1)
            {
                table = new ReadOnlyDictionary<TKey, TValue>(func(new Dictionary<TKey, TValue>()));
            }
            else
            {
                table = new ReadOnlyDictionary<TKey, TValue>(func(new Dictionary<TKey, TValue>(Capacity)));
            }
            initialized = true;

            return this;
        }

        public Pair<TKey, TValue> GetPair(TKey key)
        {
            bool success = table.TryGetValue(key, out TValue? value);
            if (!success)
                throw new KeyNotFoundException();
            return new Pair<TKey, TValue> { Key = key, Value = value };
        }

        public TValue Get(TKey key) 
        {
            bool success = table.TryGetValue(key, out TValue? value);
            if (!success)
                throw new KeyNotFoundException();
            return value;
        }

        public bool TryGet(TKey key, out Pair<TKey, TValue> pair)
        {
            bool success = table.TryGetValue(key, out TValue? value);
            if (success)
            {
                pair = new Pair<TKey, TValue> { Key = key, Value = value };
                return true;
            }
            pair = new Pair<TKey, TValue> { Key = default, Value = default };
            return false;
        }

        public bool TryGet(TKey key, out TValue value) => table.TryGetValue(key, out value);
    }
}
