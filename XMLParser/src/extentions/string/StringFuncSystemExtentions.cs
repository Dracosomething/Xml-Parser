namespace XmlParser.src.extentions.@string
{
    internal static partial class StringExtentions
    {
        extension(string str)
        {
            /// <summary>
            /// Core of our system
            /// </summary>
            /// <param name="func"></param>
            /// <param name="start"></param>
            /// <returns></returns>
            // don work rn
            public Match FirstMatch(Func<string, bool> func, int start = 0, bool referce = false)
            {
                // validate the arguemnts
                if (func == null || start > str.Length)
                    return new Match { Found = false };

                int currentLength = 1;
                string sub = referce ? str : str.Substring(start, currentLength);
                int len = str.EndIndex - start;
                while (true)
                {
                    if (func(sub))
                        return new Match
                        {
                            Found = true,
                            EndIndex = referce ? str.Length : start + currentLength,
                            StartIndex = start,
                            Result = sub
                        };
                    if (currentLength + 1 >= len || (referce && sub.Length - 1 == 0))
                        return new Match { Found = false };
                    if (referce)
                        sub = sub.RemoveLast();
                    else
                        sub = str.Substring(start, ++currentLength);
                }
            }

            public bool StartsWith(Func<string, bool> func)
            {
                var match = str.FirstMatch(func);
                return match.Found && match.StartIndex == 0;
            }

            public bool AllAsString(Func<string, bool> predicate)
            {
                return str.All(c => predicate(c.ToString()));
            }
        }
    }
}
