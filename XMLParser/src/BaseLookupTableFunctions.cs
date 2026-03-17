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
            return Utils.RegexMatch(unquoted, supplier(quote));
        }

        protected bool AssertMinLength(string text, int minLen) => text.Length > minLen;

        protected bool AssertMinLength(Array arr, int minLen) => arr.Length > minLen;

        protected bool AssertContained(string text, int minLen, string start, string end) =>
            AssertMinLength(text, minLen) && text.ContainedWithin(start, end);

        protected bool AssertContainedAndUpdate(string text, int minLen, string start, string end, out string updated)
        {
            updated = string.Empty;
            return ReturnAndInitialze((out toInit) =>
            {
                toInit = text.Substring(new Range { StartIndex = text.IndexOf(start), EndIndex = text.LastIndexOf(end) });
            }, out updated, () => AssertContained(text, minLen, start, end));
        }

        protected bool AssertCharacter(string text) => text.Length == 1;

        protected bool AssertQuotedAndUpdate(string text, out string updated)
        {
            updated = string.Empty;
            char quote = text.First();
            return ReturnAndInitialze((out toInit) =>
            {
                toInit = text.Replace(quote.ToString(), "");
            }, out updated, () => text.ContainedWithin("'") || text.ContainedWithin('"'));
        }

        protected bool CheckReference(string text, Func<string, bool> func, MatchDelegate notMatch, MatchDelegate shouldMatch, out string update)
        {
            update = string.Empty;
            var match = text.FirstMatch(func);
            return ReturnAndInitialze((out updated) =>
            {
                updated = text.Remove(match.StartIndex, match.Length);
            }, out update, () => match.Found && !notMatch(match) && shouldMatch(match));
        }

        protected bool CheckReference(string text, Func<string, bool> func, int minlength, out string updated)
        {
            return CheckReference(text, func,
                match => match.StartIndex != 0,
                match => match.Length > minlength,
                out updated);
        }

        protected bool CheckAllRefernces(string text, Func<string, bool>[] funcs, int[] minlengths, out string updated)
        {
            int i = 0;
            updated = text;
            foreach (var func in funcs)
            {
                if (!CheckReference(text, func,
                match => match.StartIndex != 0,
                match => match.Length > minlengths[i++],
                out updated))
                {
                    updated = text;
                    return false;
                }
            }
            return true;
        }

        protected bool ReadOptional(string text, int startPos, Func<string, bool> func, out string updated)
        {
            updated = text;
            var match = text.FirstMatch(func);
            if (match.Found && match.StartIndex == startPos)
                updated = text.Remove(match.StartIndex, match.Length);
            return true;
        }

        protected bool ReadMultipleTokens(string toCheckString, char seperator, Func<string, bool> func)
        {
            if (!AssertMinLength(toCheckString, 1))
                return false;
            string[] items = toCheckString.Split(seperator);
            return items.Length > 1 && items.All(func);
        }

        protected bool ReturnAndInitialze(InitExpression expression, out string toInit, Func<bool> output)
        {
            toInit = string.Empty;
            if (output())
            {
                expression(out toInit);
                return true;
            }
            else
                return false;
        }
    }
}
