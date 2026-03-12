using System;
using System.Collections.Generic;
using System.Text;

namespace XmlParser.src
{
    internal class BaseLookupTableFunctions
    {
        protected bool ReadQuoted(string quoted, FormatSupplier supplier)
        {
            if (!AssertMinLength(quoted, 2) || // make shure string is at least 2 characters long
                (!quoted.ContainedWithin('"') || // if the string is not within quotes we can savely return false
                !quoted.ContainedWithin("'")))
                return false;
            char quote = quoted.First();
            string unquoted = quoted.Substring(new Range { StartIndex = 1, EndIndex = quoted.EndIndex - 1 });
            return Constants.RegexMatch(unquoted, supplier(quote));
        }

        protected bool AssertMinLength(string text, int minLen) => text.Length > minLen;

        protected bool AssertContained(string text, int minLen, string start, string end) =>
            AssertMinLength(text, minLen) && text.ContainedWithin(start, end);

        protected bool AssertContainedAndUpdate(string text, int minLen, string start, string end, out string updated)
        {
            updated = string.Empty;
            return AssertContained(text, minLen, start, end) &&
                (updated = text.Substring(new Range { StartIndex = start.EndIndex, EndIndex = end.EndIndex })) != string.Empty;
        }

        protected bool AssertCharacter(string text) => text.Length == 1;

        protected bool AssertQuotedAndUpdate(string text, out string updated)
        {
            updated = string.Empty;
            char quote = text.First();
            return text.ContainedWithin("'") || text.ContainedWithin('"') || (updated = text.Replace(quote.ToString(), "")) != string.Empty;
        }

        protected bool CheckReference(string text, Func<string, bool> func, MatchDelegate notMatch, MatchDelegate shouldMatch, out string update)
        {
            update = string.Empty;
            var match = text.FirstMatch(func);
            return match.Found && !notMatch(match) && shouldMatch(match) &&
                (update = text.Remove(match.StartIndex, match.Length)) != string.Empty;
        }

        protected bool ReadOptional(string text, int startPos, Func<string, bool> func, out string updated)
        {
            updated = string.Empty;
            var match = text.FirstMatch(func);
            if (match.Found && match.StartIndex == startPos)
            {
                updated = text.Remove(match.StartIndex, match.Length);
                return true;
            }
            return false;
        }
    }
}
