using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using XmlParser.src.gui;
using XmlParser.src;
using static System.Net.Mime.MediaTypeNames;

namespace XmlParser.src
{
    internal static class Constants
    {
        // made static since we only need one colorscheme for the entire program.
        public static readonly ColorScheme colorScheme = new ColorScheme();

        // For http requests
        public static readonly HttpClient httpClient = new HttpClient();

        // url regex
        public static readonly string url = "((([A-Za-z]{3,9}:(?:\\/\\/)?)(?:[-;:&=\\+\\$,\\w]+@)?[A-Za-z0-9.-]+|(?:www\\.|" +
            "[-;:&=\\+\\$,\\w]+@)[A-Za-z0-9.-]+)((?:\\/[\\+~%\\/.\\w-_]*)?\\??(?:[-\\+=&;%@.\\w_]*)#?(?:[\\w]*))?)";
        public static readonly string domain = "((([A-Za-z]{3,9}:(?:\\/\\/)?)(?:[-;:&=\\+\\$,\\w]+@)?[A-Za-z0-9.-]+|(?:www\\.|" +
            "[-;:&=\\+\\$,\\w]+@)[A-Za-z0-9.-]+)\\/)";

        // file regex
        public static readonly string filePath = "(^(?:[\\w]:(\\\\|\\/))(?:(?:\\.{1,2}(\\/|\\\\)))?(?:\\w+(\\/|\\\\))*)";
        public static readonly string file = $"(^(?:[\\w]:(\\\\|\\/))(?:(?:\\.{{1,2}}(\\/|\\\\)))?(?:\\w+(\\/|\\\\))*([^{Path.GetInvalidFileNameChars()}]))";

        // Indexing
        public static readonly string index = @"(\[[0-9]{,2}])";

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
            for(int i = 0; i < count; i++)
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
                else if (byte_ >= 'a' && byte_ <= 'f') byte_ =  byte_ - 'a' + 10;
                else if (byte_ >= 'A' && byte_ <= 'F') byte_ = byte_ - 'A' + 10;
                result = (result << 4) | (byte_ & 0xF);
            }
            return result;
        }

        extension(string str)
        {
            public Match FirstMatch(Func<string, bool> func, int start = 0)
            {
                if (func == null || start > str.Length)
                    return new Match { Found = false };
                    
                int currentLength = 1;
                string sub = str.Substring(start, currentLength);
                int len = str.Length - start - 1;
                while (!func(sub))
                {
                    if (func(sub))
                        break;
                    if (currentLength + 1 >= len)
                        return new Match { Found = false };
                    sub = str.Substring(start, ++currentLength);
                }

                return new Match { 
                    Found = true, 
                    EndIndex = start + currentLength, 
                    StartIndex = start, 
                    Result = sub
                };
            }

            public string Substring(Func<string, bool> func, int start = 0)
            {
                var match = str.FirstMatch(func, start);
                if (match.Found)
                    return string.Empty;
                return match.Result;
            }

            public bool ContainedWithin(char c) => str.StartsWith(c) && str.EndsWith(c);

            public bool ContainedWithin(string _string) => str.StartsWith(_string) && str.EndsWith(_string);

            public bool Contains(Func<string, bool> func)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    var match = str.FirstMatch(func, i);
                    if (match.Found)
                        return true;
                }
                return false;
            }

            public bool StartsWith(Func<string, bool> func)
            {
                var match = str.FirstMatch(func);
                return match.Found && match.StartIndex == 0;
            }

            public bool EndsWith(Func<string, bool> func)
            {
                var match = str.FirstMatch(func);
                return match.Found && match.EndIndex == str.Length - 1;
            }

            // need allmatch function that checks if all characters match a function
        }
    }
}