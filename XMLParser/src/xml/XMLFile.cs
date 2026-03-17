namespace XmlParser.src.xml
{
    internal class XMLFile
    {
        public XMLMetaData? MetaData { get; init; } = null;
        public required XMLElement Root { get; init; }
        public XMLProcessingInstruction[] ProcessingInstructions { get; init; } = [];
    }
}
