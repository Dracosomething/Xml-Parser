namespace XmlParser.src.extentions.@string
{
    internal static partial class StringExtentions
    {
        extension(string str)
        {
            public int EndIndex => str.Length - 1;

            public bool ContainedWithin(char c) => str.StartsWith(c) && str.EndsWith(c);

            public bool ContainedWithin(string _string) => str.StartsWith(_string) && str.EndsWith(_string);

            public bool ContainedWithin(char start, char end) => str.StartsWith(start) && str.EndsWith(end);

            public bool ContainedWithin(string start, string end) => str.StartsWith(start) && str.EndsWith(end);

            public string Substring(Range range) => str.Substring(range.StartIndex, range.Length);

            public string RemoveLast(int length = 1)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(length, 1, "length");
                return str.Remove(str.Length - length);
            }
        }
    }
}
