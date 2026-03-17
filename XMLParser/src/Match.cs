namespace XmlParser.src
{
    internal readonly struct Match
    {
        public int StartIndex { get; init; }
        public int EndIndex { get; init; }
        public readonly int Length => EndIndex - StartIndex;
        public string Result { get; init; }
        public required bool Found { get; init; }

        public static implicit operator Range(Match m) => new Range { StartIndex = m.StartIndex, EndIndex = m.EndIndex };
    }
}
