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

            public string Remove(Range range) => str.Remove(range.StartIndex, range.Length);

            public string RemoveFirst(int length = 1)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(length, 1, nameof(length));
                return str.Remove(0, length);
            }

            public string RemoveLast(int length = 1)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(length, 1, nameof(length));
                return str.Remove(str.Length - length);
            }

            public string RemoveFirst(string toRemove)
            {
                ArgumentNullException.ThrowIfNullOrEmpty(toRemove, nameof(toRemove));
                int startIndex = str.IndexOf(toRemove);
                if (startIndex == -1)
                    return str;
                return str.Remove(startIndex, toRemove.Length);
            }

            public string RemoveLast(string toRemove)
            {
                ArgumentNullException.ThrowIfNullOrEmpty(toRemove, nameof(toRemove));
                int startIndex = str.LastIndexOf(toRemove);
                if (startIndex == -1)
                    return str;
                return str.Remove(startIndex, toRemove.Length);
            }

            public bool IsNumeric() => str.All(char.IsNumber);

            public bool StartsWithAny(ReadOnlySpan<char> sequence)
            {
                string regex = $"[{sequence}]+";
                var match = Regex.Match(str, regex);
                return match.Success && match.Index == 0;
            }
        }
    }
}
