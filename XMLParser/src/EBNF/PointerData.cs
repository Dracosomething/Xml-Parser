using System;
using System.Collections.Generic;
using System.Text;
using XmlParser.src.EBNF;

namespace XmlParser.src.EBNF
{
    internal class PointerData
    {
        public int IndexInGroup { get; init; }
        public int GroupIndex { get; init; }
        public EBNFQuantifier Quantifier { get; init; }
        public string PointerName { get; init; }

        public string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("index in group: ").Append(IndexInGroup).AppendLine();
            builder.Append("group index: ").Append(GroupIndex).AppendLine();
            builder.Append("quantifier: ").Append(Quantifier).AppendLine();
            builder.Append("pointer name: ").Append(PointerName).AppendLine();
            return builder.ToString();
        }
    }
}
