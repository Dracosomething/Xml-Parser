using XmlParser.src.xml;

namespace XmlParser.src.extentions
{
    internal static class WhiteSpaceHandlingExtentions
    {
        extension(WhiteSpaceHandling handling)
        {
            public static WhiteSpaceHandling FromString(string text) => text.ToLower() switch
            {
                "default" => WhiteSpaceHandling.DEFAULT,
                "preserved" => WhiteSpaceHandling.PRESERVED,
                _ => throw new ArgumentException($"{text} is not a valid method of whitespace handling.")
            };
        }
    }
}
