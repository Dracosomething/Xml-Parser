using System;
using System.Collections.Generic;
using System.Text;
using xml_parser.src.EBNF;

namespace XMLParser.src.EBNF
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

        public bool Check(char check)
        {
            bool success;
            bool result = false;
            switch(Rule)
            {
                case EBNFToken.COLLECTION:
                    success = GetData(out EBNFCollection collection);
                    if (!success)
                        return false;
                    return collection.Check(check);
                case EBNFToken.GROUP:
                    success = GetData(out EBNFTokens group);
                    if (!success)
                        return false;
                    return group.Validate(check.ToString());
                case EBNFToken.HEXADECIMAL:
                    success = GetData(out int number);
                    if (!success)
                        return false;
                    return check == number;
                case EBNFToken.REFERENCE:
                    success = GetData(out EBNFTokens reference);
                    if (!success)
                        return false;
                    return reference.Validate(check.ToString());
            }
            return false;
        }

        public bool Check(string check, ref int index)
        {
            string output;
            var success = GetData(out output);
            if (!success)
                return false;
            var result = check.Substring(--index, output.Length) == output;
            index += output.Length;
            return result;
        }
    }
}
