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
    }
}
