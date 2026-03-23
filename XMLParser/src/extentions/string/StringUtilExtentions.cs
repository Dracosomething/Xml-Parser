using System.Text.RegularExpressions;

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

            public bool ContainedWithin(ReadOnlySpan<char> chars) => str.StartsWith(chars) && str.EndsWith(chars);

            public bool ContainedWithin(ReadOnlySpan<char> start, ReadOnlySpan<char> end) => str.StartsWith(start) && str.EndsWith(end);

            public string Substring(Range range) => str.Substring(range.StartIndex, range.Length);

            public string RemoveLast(int length = 1)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(length, 1, "length");
                return str.Remove(str.Length - length);
            }

            public string RemoveFirst(string toRemove)
            {
                ArgumentNullException.ThrowIfNullOrEmpty(toRemove, "toRemove");
                int startIndex = str.IndexOf(toRemove);
                return str.Remove(startIndex, toRemove.Length);
            }

            public string RemoveLast(string toRemove)
            {
                ArgumentNullException.ThrowIfNullOrEmpty(toRemove, "toRemove");
                int startIndex = str.LastIndexOf(toRemove);
                return str.Remove(startIndex, toRemove.Length);
            }

            public bool IsNumeric() => str.All(char.IsNumber);

            public bool StartsWithAny(ReadOnlySpan<char> sequence)
            {
                string regex = "[";
                for (int i = 0; i < sequence.Length; i++)
                    regex += sequence[i];
                regex += "]";
                var match = Regex.Match(str, regex);
                return match.Success && match.Index == 0;
            }
        }
    }
}
