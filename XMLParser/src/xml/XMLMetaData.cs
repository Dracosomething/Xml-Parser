using System.Text;

namespace XMLParser.src.xml
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
    }
}
