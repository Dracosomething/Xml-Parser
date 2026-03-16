namespace XMLParser.src.xml
{
    internal class XMLProcessingInstruction
    {
        public required string PITarget { get; init; }
        public string Data { get; init; } = string.Empty;
    }
}
