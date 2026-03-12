using System;
using System.Collections.Generic;
using System.Text;

namespace XmlParser.src
{
    internal struct Range
    {
        public required int StartIndex { get; init; }
        public required int EndIndex { get; init; }

        public readonly int Length => EndIndex - StartIndex;
    }
}
