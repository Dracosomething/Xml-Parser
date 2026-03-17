using XmlParser.src.xml;

namespace XmlParser.src.extentions
{
    internal static class XMLVersionExtentions
    {
        extension(XMLVersion ver)
        {
            public static XMLVersion FromString(string text) => text switch
            {
                "1.1" => XMLVersion.XML11,
                _ => XMLVersion.XML10
            };
        }
    }
}
