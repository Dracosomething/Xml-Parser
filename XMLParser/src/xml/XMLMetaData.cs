using System.Text;

namespace XmlParser.src.xml
{
    enum XMLVersion
    {
        XML10,
        XML11
    }

    internal class XMLMetaData
    {
        public required XMLVersion Version { get; init; }
        public Encoding Encoding { get; init; } = Encoding.UTF8;
        public bool Standalone { get; init; } = true;

        public override string ToString()
        {
            var builder = new StringBuilder("{ ");

            builder
                .Append("Version: ")
                .Append(Version.ToString())
                .Append(", Encoding: ")
                .Append(Encoding.EncodingName)
                .Append(", Standalone: ")
                .Append(Standalone);

            return builder.Append(" }").ToString();
        }
    }
}
