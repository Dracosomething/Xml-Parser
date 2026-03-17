using System.Globalization;

namespace XmlParser.src.xml
{
    enum WhiteSpaceHandling
    {
        DEFAULT,
        PRESERVED
    }

    internal class XMLElement
    {
        public required string Name { get; init; }
        public Dictionary<string, string> Attributes { get; init; } = new();
        public CultureInfo? Language { get; init; } = null;
        public WhiteSpaceHandling? Space { get; init; } = null;
        public string UnparsedContent { get; init; } = string.Empty;
        public XMLData? ParsedContent { get; init; } = null;
    }
}
