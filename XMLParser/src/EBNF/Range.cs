using System;
using System.Collections.Generic;
using System.Text;

namespace XMLParser.src.EBNF
{
    internal struct Range
    {
        public int Start { get; init; }
        public int End { get; init; }

        public bool IsBetween(int num)
        {
            return num > Start && num < End;
        }
    }
}
