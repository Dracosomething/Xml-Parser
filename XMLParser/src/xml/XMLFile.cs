using System.Text;
using XmlParser.src.extentions;

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
                .Append(ProcessingInstructions.AsString());

            return builder.Append(" }").ToString();
        }
    }
}
