using System;
using System.Collections.Generic;
using System.Text;

namespace XmlParser.src
{
    internal struct Match
    {
        public int StartIndex { get; init; }
        public int EndIndex { get; init; }
        public readonly int Length => EndIndex - StartIndex;
        public string Result { get; init; }
        public required bool Found { get; init; }
    }
}
