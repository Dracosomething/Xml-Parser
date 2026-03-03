using System;
using System.Collections.Generic;
using System.Text;

namespace XmlParser.src.EBNF
{
    internal struct Range
    {
        public int Start { get; init; }
        public int End { get; init; }

        public bool IsBetween(int num)
        {
            return num >= Start && num <= End;
        }

        public string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("start: ").Append(Start).Append(" - ");
            builder.Append("end: ").Append(End).AppendLine();
            return builder.ToString();
        }
    }
}
