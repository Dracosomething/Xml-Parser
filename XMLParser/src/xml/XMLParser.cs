using System.Data;
using System.Globalization;
using System.Text;
using XmlParser.src.extentions;
using XmlParser.src.extentions.@string;

namespace XmlParser.src.xml
{
    public class XMLParser
    {
        private readonly FileInfo file;
        private GenericXMLLookupTable genericTable = new(true);
        private XMLLookupTable table = new(true);
        private List<XMLProcessingInstruction> processingInstructions = new();

        public XMLParser(string path)
        {
            file = new(path);
        }

        public XMLParser(FileInfo info)
        {
            file = info;
        }

        internal XMLFile Parse()
        {
            string text = PreProcess(Normalize(File.ReadAllText(file.FullName)));
            var readProlog = table["prolog"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("prolog"));
            if (!text.StartsWith(readProlog))
                throw new FormatException("File does not start with a prologue.");
            var reader = new FileReader(text);
            string prolog = reader.Read(readProlog);
            var metaData = ParseProlog(prolog);

            var readBody = table["element"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("element"));
            string body = reader.Read(readBody);
            var rootElement = ParseBody(body);
            return new XMLFile
            {
                Root = rootElement,
                MetaData = metaData,
                ProcessingInstructions = this.processingInstructions.ToArray()
            };
        }

        private string Normalize(string text) =>
            Utils.RegexReplace(text.Replace("\r\n", "\n"), "\n", @"\r(?!\n)");

        private string PreProcess(string text) =>
            text.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "'").Replace("&quot;", "\"");

        private void ParseMisc(ref readonly FileReader reader)
        {
            var readMisc = genericTable["Misc"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("Misc"));
            var readSpace = genericTable["S"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("S"));
            var readComment = genericTable["comment"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("comment"));
            var readPI = genericTable["PI"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("PI"));
            while (!reader.EndOfFile)
            {
                string line = reader.Read(readMisc);
                switch (line)
                {
                    case var _ when readPI(line):
                        ParseProcessingInstruction(line);
                        break;
                    case var _ when readSpace(line) || readComment(line):
                        break;
                    default:
                        throw new FormatException("Misc was not in the correct format.");
                }
            }
        }

        private XMLMetaData ParseProlog(string text)
        {
            var readXmlDecl = table["XMLDecl"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("XMLDecl"));
            var reader = new FileReader(text);
            string xmlDecl = reader.Read(readXmlDecl);

            XMLVersion version = XMLVersion.XML10;
            Encoding encoding = Encoding.UTF8;
            bool standalone = true;
            if (readXmlDecl(xmlDecl))
            {
                xmlDecl = xmlDecl.Substring(5, xmlDecl.EndIndex);
                var declarationReader = new FileReader(xmlDecl);

                var readVerInfo = genericTable["VersionInfo"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("VersionInfo"));
                var readEncoding = genericTable["EncodingDecl"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("EncodingDecl"));
                var readStandAlone = genericTable["SDDecl"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("SDDecl"));

                string versionInfo = declarationReader.Read(readVerInfo);
                if (!readVerInfo(versionInfo))
                    throw new FormatException("Could not find version info in xml decleration. versionInfo:" + versionInfo);
                version = ReadVersion(versionInfo);

                string encodingDecleration = declarationReader.Read(readEncoding);
                if (readEncoding(encodingDecleration))
                    encoding = ReadEncoding(encodingDecleration);

                string standaloneDecleration = declarationReader.Read(readStandAlone);
                if (readStandAlone(standaloneDecleration))
                    standalone = ReadStandalone(standaloneDecleration);
            }

            ParseMisc(ref reader);

            return new XMLMetaData { Version = version, Encoding = encoding, Standalone = standalone };
        }

        private XMLVersion ReadVersion(string versionInfo) =>
            ReadMetaDataAttributeValue(versionInfo, "VersionInfo", XMLVersion.FromString);

        private Encoding ReadEncoding(string encodingDecleration) =>
            ReadMetaDataAttributeValue(encodingDecleration, "EncodingDecl", Encoding.GetEncoding);

        private bool ReadStandalone(string standaloneDecleration) =>
            ReadMetaDataAttributeValue(standaloneDecleration, "SDDecl", (value) => value.Replace("'", "").Replace("\"", "") == "yes");

        private XMLElement ParseBody(string body) =>
            ParseElement(body);

        private XMLElement ParseElement(string element)
        {
            var readElement = table["element"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("element"));
            if (!readElement(element))
                throw new FormatException("Body is not in the correct format. element:" + element);

            var emptyElement = table["EmptyElemTag"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("EmptyElemTag"));
            var reader = new FileReader(element);
            if (emptyElement(element))
                return ParseOpeningTag(element, in reader);

            var readStartTag = table["STag"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("STag"));
            string openingTag = reader.Read(readStartTag);
            if (!readStartTag(openingTag))
                throw new FormatException("openingTag is not int the correct format for a starting tag. openingTag:" + openingTag);

            var startingTagData = ParseOpeningTag(openingTag, in reader);
            var readEndingTag = table["ETag"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("ETag"));

            string leftOverData = reader.LeftOver;
            // remove spaces and closing '>'
            leftOverData = leftOverData.TrimStart(Constants.whiteSpace).Remove(0, 1);
            if (readEndingTag(leftOverData))
                return startingTagData;

            reader = new FileReader(leftOverData);
            var readContent = table["content"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("content"));
            string body = reader.Read(readContent);
            if (!readContent(body))
                throw new FormatException("content is in the wrong format. body:" + body);
            var data = ParseXMLElementContent(body);
            return new XMLElement
            {
                Name = startingTagData.Name,
                Attributes = startingTagData.Attributes,
                Language = startingTagData.Language,
                Space = startingTagData.Space,
                UnparsedContent = body,
                ParsedContent = data
            };
        }

        private XMLElement ParseOpeningTag(string tag, in FileReader reader)
        {
            var readName = genericTable["Name"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("Name"));

            string name = reader.Read(readName);
            string attributesAsString = reader.LeftOver.RemoveLast(2);
            var attributes = ParseAttributes(attributesAsString);

            CultureInfo? language = null;
            if (attributes.ContainsKey("xml:lang"))
            {
                language = CultureInfo.GetCultureInfo(attributes["xml:lang"]);
                attributes.Remove("xml:lang");
            }

            WhiteSpaceHandling? handling = null;
            if (attributes.ContainsKey("xml:space"))
            {
                handling = WhiteSpaceHandling.FromString(attributes["xml:space"]);
                attributes.Remove("xml:space");
            }

            return new XMLElement { Name = name, Attributes = attributes, Language = language, Space = handling };
        }

        private Dictionary<string, string> ParseAttributes(string attributes)
        {
            var dic = new Dictionary<string, string>();
            string[] attributesAsArray = attributes.Split(Constants.whiteSpace, StringSplitOptions.RemoveEmptyEntries);
            // walk through attriutesAsArray with for loop and parse em individually
            for (int i = 0; i < attributesAsArray.Length; i++)
                dic.Add(ParseAttribute(attributesAsArray[i], in dic));
            return dic;
        }

        private Pair<string, string> ParseAttribute(string attribute, ref readonly Dictionary<string, string> dic)
        {
            string[] pair = attribute.Split('=', 1);
            ArgumentOutOfRangeException.ThrowIfNotEqual(pair.Length, 2, "pair");
            string name = pair[0].TrimEnd(Constants.whiteSpace);
            if (dic.ContainsKey(name))
                throw new DuplicateNameException($"Key: {name} already exists on attribute.");
            string valueQuoted = pair[1].TrimStart(Constants.whiteSpace);
            string value = valueQuoted.Substring(new Range { StartIndex = 1, EndIndex = valueQuoted.EndIndex - 1 });
            return new Pair<string, string> { Key = name, Value = value };
        }

        private XMLData ParseXMLElementContent(string content)
        {
            var readContent = table["content"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("content"));
            if (!readContent(content))
                throw new FormatException("content is in the wrong format. content:" + content);

            // we want to handle the character data like normal text
            // any encountered xml elements should be parsed on their own and then added to the elements
            // all the character data sections should be treated like normal text
            // processing instructions should be treated like normal processing instructions
            // comments should be ignored
            var readComment = genericTable["comment"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("comment"));
            var readPI = genericTable["PI"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("PI"));
            var readElement = table["element"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("element"));
            var readCDSect = table["CDSect"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("CDSect"));
            bool readData(string line) => readComment(line) || readPI(line) || readCDSect(line) || readElement(line);

            var readCharData = table["CharData"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("CharData"));
            var reader = new FileReader(content);
            string text = "";
            List<XMLElement> elements = new();
            while (!reader.EndOfFile)
            {
                if (reader.LeftOver.StartsWith(readCharData))
                {
                    text += reader.Read(readCharData);
                    continue;
                }
                string data = reader.Read(readData);
                if (!readData(data))
                    throw new FormatException("data is not a comment, processing instruction, element or character data section. data:" + data);
                switch (data)
                {
                    case var _ when readPI(data):
                        ParseProcessingInstruction(data);
                        break;
                    case var _ when readCDSect(data):
                        text += ParseCharacterDataSection(data);
                        break;
                    case var _ when readElement(data):
                        elements.Add(ParseElement(data));
                        break;
                    case var _ when readComment(data):
                        continue;
                }
            }
            return new XMLData { Text = text, Children = elements };
        }

        private void ParseProcessingInstruction(string instruction)
        {
            var readPI = genericTable["PI"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("PI"));
            if (!readPI(instruction))
                throw new FormatException("Processing instruction was in an invalid format. Instruction:" + instruction);
            var match = instruction.FirstMatch(genericTable["PITarget"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("PITarget")));
            if (!match.Found || match.StartIndex < 2)
                throw new FormatException("Processing instruction does not have a target at the corrent position. Instruction:" + instruction);
            string name = instruction.Substring(match);
            string body = instruction.Substring(match.EndIndex).Replace("?>", "");
            this.processingInstructions.Add(new XMLProcessingInstruction { PITarget = name, Data = body });
        }

        private string ParseCharacterDataSection(string section)
        {
            if (!section.StartsWith("<![CDATA[") || !section.EndsWith("]]>"))
                throw new FormatException("section does not start with \"<![CDATA[\" or does not end with \"]]>\". section:" + section);
            string toReturn = section.RemoveFirst("<![CDATA[").RemoveFirst("]]>");
            return toReturn.Contains("]]>") ?
                throw new FormatException("toReturn contains \"]]>\" which it is not allowed to contain. toReturn:" + toReturn) :
                toReturn;
        }

        private TResult ReadMetaDataAttributeValue<TResult>(string attribute, string tableKey, Func<string, TResult> output)
        {
            var func = genericTable[tableKey] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage(tableKey));
            if (!func(attribute))
                throw new FormatException($"{tableKey} is not in a valid format. attribute:" + attribute);
            int equalsIndex = attribute.IndexOf('=');
            string data = attribute.Substring(equalsIndex).TrimStart(Constants.whiteSpace);
            return output(data);
        }
    }
}
