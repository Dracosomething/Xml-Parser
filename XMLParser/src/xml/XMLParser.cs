using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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
            // remove the pre process from here and call it on the text where a reference is possible
            string text = RemoveComments(Normalize(File.ReadAllText(file.FullName)));

            var reader = new FileReader(text);

            // parse the xml decleration
            XMLMetaData? metaData = null;
            if (HasXMLDecleration(text))
                metaData = ParseXMLDecleration(ref reader);

            // handle possible new string encoding
            string leftOver = reader.LeftOver;
            if (metaData != null)
            {
                byte[] bytes = Encoding.Default.GetBytes(leftOver);
                leftOver = metaData.Encoding.GetString(bytes);
            }

            // process all the processing instructions
            string body = ProcessProcessingInstructions(leftOver).Trim(Constants.whiteSpace); // process all processing instructions,
                                                                                              // after this there can be no more processing
                                                                                              // instructions in the file
            var rootElement = ParseBody(body);
            return new XMLFile
            {
                Root = rootElement,
                MetaData = metaData,
                ProcessingInstructions = this.processingInstructions.ToArray()
            };
        }

        #region Pre parsing
        private string Normalize(string text) =>
            Utils.RegexReplace(text.Replace("\r\n", "\n"), "\n", @"\r(?!\n)");

        private string PreProcess(string text) =>
            text.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "'").Replace("&quot;", "\"");

        private string RemoveComments(string text)
        {
            var readComment = genericTable["comment"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("comment"));
            while (true)
            {
                int commentStart = text.IndexOf("<!--");
                if (commentStart == -1)
                    return text;
                int commentEnd = text.IndexOf("-->");
                if (commentEnd <= commentStart || commentEnd == -1)
                    throw new FormatException($"Encountered a comment in the file that was not in the correct syntax. " +
                        $"commentStart:{commentStart}, commentEnd:{commentEnd}, text:{text}");
                string comment = text.Substring(new Range { StartIndex = commentStart, EndIndex = commentEnd + 3 });
                if (!readComment(comment))
                    throw new FormatException($"Encountered a comment in the file that was not in the correct syntax. " +
                        $"commentStart:{commentStart}, commentEnd:{commentEnd}, comment:{comment}, text:{text}");
                text = text.Replace(comment, "");
            }
        }

        private string ProcessProcessingInstructions(string text)
        {
            while (true)
            {
                int startIndex = text.IndexOf("<?");
                if (startIndex == -1)
                    return text;
                int endIndex = text.IndexOf("?>");
                if (endIndex <= startIndex || endIndex == -1)
                    throw new FormatException($"Couldn't find the end of a processing instruction. startIndex:{startIndex}, endIndex:{endIndex}, text:{text}");
                string processingInstruction = text.Substring(new Range { StartIndex = startIndex, EndIndex = endIndex + 2 });
                ParseProcessingInstruction(processingInstruction);
                text = text.Replace(processingInstruction, "").Trim(Constants.whiteSpace);
            }
        }
        #endregion

        #region Metadata
        private XMLMetaData ParseXMLDecleration(ref readonly FileReader reader)
        {
            var readVerInfo = genericTable["VersionInfo"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("VersionInfo"));
            var readEncoding = genericTable["EncodingDecl"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("EncodingDecl"));
            var readStandAlone = table["SDDecl"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("SDDecl"));

            // initialize required variables
            XMLVersion version = XMLVersion.XML10;
            Encoding encoding = Encoding.UTF8;
            bool standalone = true;
            if (!reader.Skip(5))
                throw new FormatException("The xml decleration does not have the correct length.");

            string versionInfo = reader.Read(readVerInfo);
            if (!readVerInfo(versionInfo))
                throw new FormatException("Could not find version info in xml decleration. versionInfo:" + versionInfo);
            version = ReadVersion(versionInfo);
            if (version == XMLVersion.XML11)
            {
                this.genericTable = new(false);
                this.table = new(false);
            }

            string encodingDecleration = reader.Read(readEncoding);
            if (readEncoding(encodingDecleration))
                encoding = ReadEncoding(encodingDecleration);

            string standaloneDecleration = reader.Read(readStandAlone);
            if (readStandAlone(standaloneDecleration))
                standalone = ReadStandalone(standaloneDecleration);

            reader.Trim();
            reader.Skip(2);

            return new XMLMetaData { Version = version, Encoding = encoding, Standalone = standalone };
        }

        private XMLVersion ReadVersion(string versionInfo) =>
            ReadMetaDataAttributeValue(versionInfo, "VersionInfo", XMLVersion.FromString);

        private Encoding ReadEncoding(string encodingDecleration) =>
            ReadMetaDataAttributeValue(encodingDecleration, "EncodingDecl", Encoding.GetEncoding);

        private bool ReadStandalone(string standaloneDecleration) =>
            ReadMetaDataAttributeValue(standaloneDecleration, "SDDecl", (value) => value == "yes");
        #endregion

        #region Parsing
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
                return ParseOpeningTag(element);

            var readStartTag = table["STag"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("STag"));
            string openingTag = reader.Read(readStartTag);
            if (!readStartTag(openingTag))
                throw new FormatException("openingTag is not int the correct format for a starting tag. openingTag:" + openingTag);

            var startingTagData = ParseOpeningTag(openingTag);
            var readEndingTag = table["ETag"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("ETag"));

            string leftOverData = reader.LeftOver;
            // remove spaces and closing '>'
            if (readEndingTag(leftOverData))
                return startingTagData;

            // get the last index of "</"
            int endIndex = leftOverData.LastIndexOf("</");
            if (endIndex == -1)
                throw new FormatException("No closing tag could be found. element:" + element);

            var readContent = table["content"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("content"));
            string body = leftOverData.Substring(0, endIndex);
            if (!readContent(body))
                throw new FormatException("content is in the wrong format. body:" + body);
            // parse the elements content
            var data = ParseXMLElementContent(body, startingTagData.Space);

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

        private XMLElement ParseOpeningTag(string tag)
        {
            var readName = genericTable["Name"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("Name"));

            // todo: fix this function by having it split at the whitespace, removing empty entries and only allowing 2 entries
            string[] pair = tag.Split(Constants.whiteSpace, 2, StringSplitOptions.RemoveEmptyEntries);
            if (pair.Length == 1)
            {
                return new XMLElement
                {
                    // it just has a name
                    Name = tag.RemoveFirst().RemoveLast().TrimEnd(Constants.whiteSpace)
                };
            }

            string name = pair[0].RemoveFirst();
            if (!readName(name))
                throw new FormatException("name is not using the characters allowed in names. name:" + name);
            string attributesAsString = pair[1];
            var attributes = ParseAttributes(attributesAsString);

            CultureInfo? language = null;
            if (attributes.ContainsKey("xml:lang"))
            {
                language = CultureInfo.GetCultureInfo(attributes["xml:lang"]);
                attributes.Remove("xml:lang");
            }

            WhiteSpaceHandling handling = WhiteSpaceHandling.DEFAULT;
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
            // walk through attriutesAsArray with for loop and parse em individually
            while (true)
            {
                // get the position at where we'll split
                int equalsIndex = attributes.IndexOf('=');
                if (equalsIndex == -1)
                    break;
                string tmp = attributes.Substring(equalsIndex);
                // get the value of the attribute and validate it
                char quote = tmp.RemoveFirst().TrimStart(Constants.whiteSpace).First();
                int valueLen = tmp.IndexOf(quote, tmp.IndexOf(quote) + 1) + 1;
                // update body again
                int length = equalsIndex + valueLen;
                string attribute = attributes.Substring(0, length);
                dic.Add(ParseAttribute(attribute, ref dic));
                attributes = attributes.RemoveFirst(attribute).TrimStart(Constants.whiteSpace);
            }
            return dic;
        }

        private Pair<string, string> ParseAttribute(string attribute, ref readonly Dictionary<string, string> dic)
        {
            string[] pair = attribute.Split('=', 2);
            ArgumentOutOfRangeException.ThrowIfNotEqual(pair.Length, 2, "pair");
            string name = pair[0].TrimEnd(Constants.whiteSpace);
            if (dic.ContainsKey(name))
                throw new DuplicateNameException($"Key: {name} already exists on attribute.");
            string valueQuoted = pair[1].TrimStart(Constants.whiteSpace);
            string value = PreProcess(valueQuoted.Substring(new Range { StartIndex = 1, EndIndex = valueQuoted.EndIndex }));
            return new Pair<string, string> { Key = name, Value = value };
        }

        private XMLData ParseXMLElementContent(string content, WhiteSpaceHandling whiteSpaceHandling)
        {
            var readContent = table["content"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("content"));
            if (!readContent(content))
                throw new FormatException("content is in the wrong format. content:" + content);

            string text = "";
            List<XMLElement> elements = new();
            /* stream for parsing element content
             *  loop untill everything is parsed
             *      get index of '<'
             *      if index is -1
             *          just parse everything as normal text, preprocess and handle whitespace properly
             *      end if
             *      get everything before index
             *      parse before as normal text
             *      if character after index is '!'
             *          get everything inbetween <![CDATA[ and ]]>
             *          parse that as normal text without any extra processing
             *          remove section from content
             *      otherwise
             *          read the entire tag
             *          if the tag ends with '/>'
             *              parse tag and add to elements
             *          otherwise
             *              get index of closing tag and of next opening tag
             *              if opening tag is before closing tag and there is another opening tag
             *                  read each nested opening tag untill the correct closing tag
             *              otherwise
             *                  extract the element and parse it
             *              end if
             *          end if
             *          remove element from content
             *      end if
             *      remove normal text from content
             *  end loop
             */
            for (int i = 0; i < content.Length; i++)
            {
                int tagIndex = content.IndexOf('<');
                if (tagIndex == -1)
                {
                    text += FormatTextData(content, whiteSpaceHandling);
                    break;
                }
                string textData = content.Substring(0, tagIndex);
                text += FormatTextData(textData, whiteSpaceHandling);
                if (content[tagIndex + 1] == '!')
                {
                    int closingIndex = content.IndexOf("]]>");
                    if (closingIndex == -1)
                        throw new FormatException("CDATA section does not close. content:" + content);
                    string section = content.Substring(new Range { StartIndex = tagIndex, EndIndex = closingIndex + 3 });
                    string data = ParseCharacterDataSection(section);
                    text += data;
                    content = content.RemoveFirst(section);
                }
                else
                {
                    int closingIndex = content.IndexOf(">");
                    if (closingIndex == -1)
                        throw new FormatException("Element does not have a closing '>'. content:" + content);
                    string tag = content.Substring(new Range { StartIndex = tagIndex, EndIndex = closingIndex + 1 });
                    if (tag.EndsWith("/>"))
                        elements.Add(ParseOpeningTag(tag));
                    else
                    {
                        tag = ExtractChildElement(content, tag);
                        elements.Add(ParseElement(tag));
                    }
                    content = content.RemoveFirst(tag);
                }
                content = content.RemoveFirst(textData);
            }

            return new XMLData { Text = text, Children = elements };
        }

        private void ParseProcessingInstruction(string instruction)
        {
            var readPI = genericTable["PI"] ?? throw new KeyNotFoundException(Utils.GeneralizedKeyNotFoundMessage("PI"));
            if (!readPI(instruction))
                throw new FormatException("Processing instruction was in an invalid format. Instruction:" + instruction);
            instruction = instruction.RemoveFirst("<?").RemoveLast("?>");
            string[] tokens = instruction.Split(Constants.whiteSpace, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 1)
                throw new FormatException("Processing instruction does not have a target at the corrent position. Instruction:" + instruction);
            string name = tokens[0];
            string body = instruction.RemoveFirst(name);
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
            string data = attribute.Substring(equalsIndex + 1).TrimStart(Constants.whiteSpace).RemoveFirst().RemoveLast();
            return output(data);
        }
        #endregion

        private bool HasXMLDecleration(string text)
        {
            bool startsWithxml = text.StartsWith("<?xml");
            string startOfData = text.RemoveFirst("<?xml");
            return startsWithxml && startOfData.StartsWithAny(Constants.whiteSpace);
        }

        private string FormatTextData(string textData, WhiteSpaceHandling whiteSpaceHandling)
        {
            textData = PreProcess(textData);
            if (whiteSpaceHandling == WhiteSpaceHandling.DEFAULT)
                textData = Regex.Replace(textData, @"[\s\n\r\t]+", " ");
            return textData;
        }

        private string ExtractChildElement(string parentBody, string tag)
        {
            // making everything ready for the actual meat of this function
            int tagIndex = parentBody.IndexOf(tag);
            if (tagIndex == -1)
                throw new ArgumentException($"Argument:tag, value:{tag} does not exist in parentBody. parentBody:{parentBody}");
            string tagName = tag.Split(Constants.whiteSpace, 2, StringSplitOptions.RemoveEmptyEntries)[0].RemoveFirst().TrimEnd(Constants.whiteSpace);
            string leftOver = parentBody.Substring(tagIndex + tag.Length);
            string closingTag = $"</{tagName}";
            string openingTag = $"<{tagName}";
            /*
             *  get index of closing tag and of next opening tag
             *  if opening tag is before closing tag and there is another opening tag
             *      do
             *          read index of opening tag
             *          read index of closing tag
             *          if opening tag is before closing tag
             *              decrement nesting count by 1
             *              skip closing tag
             *          otherwise
             *              increment nesting count by 1
             *              skip opening tag
             *          end if
             *      while nesting count is 0 or less
             *      extract from tag index to closing tag index
             *  otherwise
             *      from closing index get first index of '>'
             *      add closing index to '>' index
             *      extract from leftOver untill end index
             *      combine tag and body
             *  end if
             */
            int closingIndex = leftOver.IndexOf(closingTag);
            int openingIndex = leftOver.IndexOf(openingTag);
            if (openingIndex != -1 || openingIndex > closingIndex)
            {
                int nestingCount = 1;
                int endIndex;
                var reader = new FileReader(leftOver);
                do
                {
                    string tmp = reader.LeftOver;
                    openingIndex = tmp.IndexOf(openingTag);
                    closingIndex = tmp.IndexOf(closingTag);
                    endIndex = tmp.IndexOf('>') + 1;
                    if (closingIndex > openingIndex)
                    {
                        nestingCount++;
                        reader.Skip(new Range { StartIndex = openingIndex, EndIndex = endIndex });
                    }
                    else
                    {
                        nestingCount--;
                        reader.Skip(new Range { StartIndex = closingIndex, EndIndex = endIndex });
                    }
                } while (nestingCount > 0);
                return parentBody.Substring(new Range { StartIndex = tagIndex, EndIndex = reader.Index + tag.Length + tagIndex });
            }
            else
            {
                int endIndex = leftOver.Substring(closingIndex).IndexOf('>');
                endIndex += closingIndex;
                return tag + leftOver.Substring(0, endIndex + 1);
            }
        }
    }
}
