namespace XmlParser.src.extentions
{
    internal static class StringExtentions
    {
        extension(string str)
        {
            public int EndIndex => str.Length - 1;

            /// <summary>
            /// Core of our system
            /// </summary>
            /// <param name="func"></param>
            /// <param name="start"></param>
            /// <returns></returns>
            public Match FirstMatch(Func<string, bool> func, int start = 0)
            {
                // validate the arguemnts
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

                return new Match
                {
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

            public bool ContainedWithin(char start, char end) => str.StartsWith(start) && str.EndsWith(end);

            public bool ContainedWithin(string start, string end) => str.StartsWith(start) && str.EndsWith(end);

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

            public string Substring(Range range) => str.Substring(range.StartIndex, range.Length);

            public bool AllString(Func<string, bool> predicate)
            {
                return str.All(c => predicate(c.ToString()));
            }

            public string Replace(Func<string, bool> old, string _new)
            {
                string retVal = str;
                while (true)
                {
                    var match = retVal.FirstMatch(old);
                    if (!match.Found)
                        break;
                    retVal = retVal.Remove(match.StartIndex, match.Length);
                }
                return retVal;
            }
        }
    }
}
