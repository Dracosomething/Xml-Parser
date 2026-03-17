using XmlParser.src.extentions;
using XMLParser.src.xml;

namespace XmlParser.src.xml
{
    public class XMLFileException : Exception
    {
        public XMLFileException(FileInfo file) : base($"{file.FullName} is not a XML file.") { }
    }

    public class XMLParser
    {
        private readonly FileInfo file;
        private GenericXMLLookupTable genericTable = new(true);
        private XMLLookupTable table = new(true);

        public XMLParser(string path)
        {
            file = new(path);
        }

        public XMLParser(FileInfo info)
        {
            file = info;
        }

        public void Parse()
        {
            string text = PreProcess(Normalize(File.ReadAllText(file.FullName)));
            var readProlog = table["prolog"];
            var readXmlDecleration = table["XMLDecl"];
            if ()
        }

        private string Normalize(string text) => Utils.RegexReplace(text.Replace("\r\n", "\n"), "\n", @"\r(?!\n)");

        private string PreProcess(string text) =>
            text.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "'").Replace("&quot;", "\"");

        private XMLMetaData ParseProlog(string prolog)
        {
            if (!prolog.s)
        }
    }
}
