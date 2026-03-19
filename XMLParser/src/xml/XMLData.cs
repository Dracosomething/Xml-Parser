using System.Text;

namespace XmlParser.src.xml
{
    internal class XMLData
    {
        public string Text { get; init; } = string.Empty;
        public List<XMLElement> Children { get; init; } = new();

        public override string ToString()
        {
            var builder = new StringBuilder("{ ");

            builder
                .Append("Text: ")
                .Append(Text)
                .Append(", Children: ")
                .Append(Children);
            return builder.Append(" }").ToString();
        }
    }
}
