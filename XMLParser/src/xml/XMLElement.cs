using System.Globalization;
using System.Text;
using XmlParser.src.extentions;

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
        public WhiteSpaceHandling Space { get; init; } = WhiteSpaceHandling.DEFAULT;
        public string UnparsedContent { get; init; } = string.Empty;
        public XMLData? ParsedContent { get; init; } = null;

        public override string ToString()
        {
            var builder = new StringBuilder("{ ");

            builder
                .Append("Name: ")
                .Append(Name)
                .Append(", Attributes: ")
                .Append(Attributes.AsString())
                .Append(", Language: ")
                .Append(Language == null ? "null" : Language.ToString())
                .Append(", Space: ")
                .Append(Space == null ? "null" : Space.ToString())
                .Append(", UnparsedContent: { ")
                .Append(UnparsedContent)
                .Append(" }, ParsedContent: ")
                .Append(ParsedContent);

            return builder.Append(" }").ToString();
        }
    }
}
