using System.Text;

namespace XmlParser.src.xml
{
    internal class XMLFile
    {
        public XMLMetaData? MetaData { get; init; } = null;
        public required XMLElement Root { get; init; }
        public XMLProcessingInstruction[] ProcessingInstructions { get; init; } = [];

        public override string ToString()
        {
            var builder = new StringBuilder("{ ");

            builder
                .Append("MetaData: ")
                .Append(MetaData == null ? "null" : MetaData.ToString())
                .Append(", Root: ")
                .Append(Root)
                .Append(", ProcessingInstructions: ")
                .Append(ProcessingInstructions);

            return builder.Append(" }").ToString();
        }
    }
}
