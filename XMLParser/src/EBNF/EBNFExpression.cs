using System;
using System.Collections.Generic;
using System.Text;
using XmlParser.src.EBNF;
using XmlParser.src.xml;

namespace XmlParser.src.EBNF
{
    internal class EBNFExpression
    {
        public EBNFToken Rule { get; init; }
        public string Token { get; init; }
        public Type Type { get; init; }
        public object Data { get; init; }

        public bool GetData<T>(out T output)
        {
            if (typeof(T) == Type && Data is T)
            {
                output = (T)Data;
                return true;
            }
            output = default!;
            return false;
        }

        public bool Check(string check, ref int index, ref Dictionary<Pair<int, int>, Pair<int, string>> saved)
        {
            bool success;
            int length = 0;
            switch (Rule)
            {
                case EBNFToken.STRING:
                    string output;
                    success = GetData(out output);
                    if (!success)
                        return false;
                    var result = check.Substring(index, output.Length) == output;
                    index += output.Length;
                    return result;
                case EBNFToken.COLLECTION:
                    success = GetData(out EBNFCollection collection);
                    if (!success)
                        return false;
                    return collection.Check(check[index++]);
                case EBNFToken.GROUP:
                    success = GetData(out EBNFTokens group);
                    if (!success)
                        return false;
                    return group.Validate(check.Substring(index), ref length, ref saved, );
                case EBNFToken.HEXADECIMAL:
                    success = GetData(out int number);
                    if (!success)
                        return false;
                    return check[index++] == number;
                case EBNFToken.REFERENCE:
                    success = GetData(out EBNFTokens reference);
                    if (!success)
                        return false;
                    return reference.Validate(check.Substring(index), ref length, ref saved);
            }
            index += length;
            return false;
        }

        public bool Check(string check, EBNFExpression next, int i, ref int index, ref Dictionary<Pair<int, int>, Pair<int, string>> saved)
        {
            var success = GetData(out string name);
            if (!success)
                return false;
            int length = 0;
            int startIndex = index;
            while (!next.Check(check, ref index, ref saved))
            {
                length++;
                if (index >= check.Length)
                    return false;
            }
            saved.Add(new Pair<int, int> { Key = index, Value = length}, new Pair<int, string> { Key = i, Value = name });
            return true;
        }
    }
}