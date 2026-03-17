using System.Text.RegularExpressions;

namespace XmlParser.src
{
    internal static class Utils
    {
        public static string RegexReplace(string value, string replacement, string regex)
        {
            var replace = new Regex(regex);
            return replace.Replace(value, replacement);
        }

        public static bool RegexMatch(string value, string regex)
        {
            var match = Regex.Match(value, regex);
            return match.Success && match.Length == value.Length;
        }

        public static int RegexCount(string value, string regex)
        {
            var reg = new Regex(regex);
            MatchCollection matches = reg.Matches(regex);
            return matches.Count;
        }

        public static string RegexExtract(string value, string regex) => Regex.Match(value, regex).Value;

        public static string RegexRemove(string value, string regex, int count = 1)
        {
            var newVal = value;
            var matches = Regex.Matches(value, regex);
            for (int i = 0; i < count; i++)
            {
                var match = matches[i];
                newVal = value.Remove(match.Index, match.Length);
            }
            return newVal;
        }

        public static int CountChar(string value, char needle)
        {
            int count = 0;
            foreach (char c in value)
                if (c == needle) count++;
            return count;
        }

        public static int IntFromHex(string hexadecimal)
        {
            int result = 0;
            for (int i = 0; i < hexadecimal.Length; i++)
            {
                int byte_ = hexadecimal[i];
                if (byte_ >= '0' && byte_ <= '9') byte_ -= '0';
                else if (byte_ >= 'a' && byte_ <= 'f') byte_ = byte_ - 'a' + 10;
                else if (byte_ >= 'A' && byte_ <= 'F') byte_ = byte_ - 'A' + 10;
                result = (result << 4) | (byte_ & 0xF);
            }
            return result;
        }

        public static string GeneralizedKeyNotFoundMessage(string key) => $"Could not find {key}: XMLDecl in LookupTable.";
    }
}