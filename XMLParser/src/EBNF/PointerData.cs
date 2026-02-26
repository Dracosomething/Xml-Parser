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
    }
}
