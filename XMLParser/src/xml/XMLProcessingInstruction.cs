using System.Text;

namespace XmlParser.src.xml
{
    internal class XMLProcessingInstruction
    {
        public required string PITarget { get; init; }
        public string Data { get; init; } = string.Empty;

        public override string ToString()
        {
            var builder = new StringBuilder("{ ");

            builder
                .Append("PITarget: ")
                .Append(PITarget)
                .Append(", Data: { ")
                .Append(Data)
                .Append(" }");

            return builder.Append(" }").ToString();
        }
    }
}
