namespace XmlParser.src.xml
{
    internal class XMLData
    {
        public string Text { get; init; } = string.Empty;
        public List<XMLElement> Children { get; init; } = new();
    }
}
