using System.Text.RegularExpressions;
using XmlParser.src.extentions.@string;

namespace XmlParser.src.xml
{
    internal partial class XMLLookupTable : BaseLookupTableFunctions
    {
        //[^<&]* - ([^<&]* ']]>' [^<&]*)
        private bool ReadCharacterData(string toCheckString) =>
            Utils.RegexMatch(toCheckString, "[^<&]*") && !toCheckString.Contains("]]>");

        // '<![CDATA[' (Char* - (Char* ']]>' Char*)) ']]>'
        private bool ReadCharacterDataSection(string toCheckString) =>
            AssertContainedAndUpdate(toCheckString, 13, "<![CDATA[", "]]>", out string body) &&
            (body.AllAsString(GenericXMLLookupTable["Char"]) && !body.Contains("]]>"));

        // XMLDecl? Misc* (doctypedecl Misc*)?
        // the (doctypedecl Misc*)? has not been implemented yet
        private bool ReadProlog(string toCheckString)
        {
            // we want to get rid of this call
            ReadOptional(toCheckString, 0, ReadXMLDeclreration, out string extra, true);
            extra = extra.Replace(" ", "").Replace("\n", "").Replace("\t", "").Replace("\r", "");
            if (extra.Length == 0)
                return true;
            string[] instructions = Regex.Split(extra, @"(?<=\?>)");
            return instructions.All(GenericXMLLookupTable["PI"]);
        }

        // '<?xml' VersionInfo EncodingDecl? SDDecl? S? '?>'
        private bool ReadXMLDeclreration(string toCheckString)
        {
            // min length = '<?xml'.Length + VersionInfo->minLength + '?>'.Length = 5 + 14 + 2 = 21
            if (!AssertContainedAndUpdate(toCheckString, 21, "<?xml", "?>", out string body))
                return false;
            // remove the trailing whitespace
            body = body.TrimEnd(Constants.whiteSpace);
            // get all the individual properties
            string[] tripple = Regex.Split(body, @"(?<='|\u0022)(?=[\s\t\r\n])");
            for (int i = 0; i < tripple.Length; i++)
            {
                string property = tripple[i];
                Func<string, bool> predicate;
                switch (i)
                {
                    // first one should always be VersionInfo
                    case 0:
                        predicate = GenericXMLLookupTable["VersionInfo"];
                        break;
                    // second one can be eather encoding decleration or sd decleration, find out by checking if it starts with encoding when trimed
                    case 1:
                        predicate = GenericXMLLookupTable["EncodingDecl"];
                        break;
                    case 2:
                        predicate = ReadstandaloneDocumentDecleration;
                        break;
                    default:
                        return false;
                }
                if (!predicate(property))
                    return false;
            }
            return true;
        }

        // S 'standalone' Eq (("'" ('yes' | 'no') "'") | ('"' ('yes' | 'no') '"'))
        private bool ReadstandaloneDocumentDecleration(string toCheckString) =>
            GenericXMLLookupTable.ReadDeclaration(toCheckString, "standalone", 2, (str) => str == "yes" || str == "no");

        // EmptyElemTag | STag content ETag
        private bool ReadElement(string toCheckString) =>
            ReadEmptyElementTag(toCheckString) ||
            Return(() =>
            {
                int endPosStartingTag = toCheckString.IndexOf('>');
                if (endPosStartingTag == -1)
                    return false;
                string startTag = toCheckString.Substring(0, endPosStartingTag + 1);
                if (!ReadStartTag(startTag))
                    return false;
                string contentAndEndTag = toCheckString.RemoveFirst(startTag);
                int startPosClosingTag = contentAndEndTag.LastIndexOf("<");
                if (startPosClosingTag == -1)
                    return false;
                string endTag = contentAndEndTag[startPosClosingTag..];
                if (!ReadEndTag(endTag))
                    return false;
                string content = contentAndEndTag[..startPosClosingTag];
                if (!ReadContent(content))
                    return false;
                return true;
            });

        // '<' Name (S Attribute)* S? '>'
        private bool ReadStartTag(string toCheckString) =>
            ReadTag(toCheckString, ">");

        // Name Eq AttValue
        private bool ReadAttribute(string toCheckString)
        {
            if (!AssertMinLength(toCheckString, 4))
                return false;
            string[] pair = toCheckString.Split('=', 2);
            if (pair.Length != 2)
                return false;
            string name = pair[0].Trim(Constants.whiteSpace);
            string value = pair[1].Trim(Constants.whiteSpace);
            return GenericXMLLookupTable["Name"](name) && GenericXMLLookupTable["AttValue"](value);
        }

        // '</' Name S? '>'
        private bool ReadEndTag(string toCheckString)
        {
            if (!AssertContainedAndUpdate(toCheckString, 4, "</", ">", out string name))
                return false;
            name = name.TrimEnd(Constants.whiteSpace);
            return GenericXMLLookupTable["Name"](name);
        }

        // CharData? ((element | Reference | CDSect | PI | Comment) CharData?)*
        private bool ReadContent(string toCheckString)
        {
            if (!AssertMinLength(toCheckString, 3))
                return false;
            toCheckString = GenericXMLLookupTable.ValidateReferences(toCheckString, out bool success);
            if (!success)
                return false;
            // validate the child elements and the cdata sections
            for (int i = 0; i < toCheckString.Length; i++)
            {
                // get index of '<' then read everything before it as character data, then check if we find an element or cdata section,
                // if its neather we read it as character data
                int sectionindex = toCheckString.IndexOf('<');
                int sectionEndIndex;
                if (sectionindex == -1)
                    return ReadCharacterData(toCheckString);
                // check if it is a cdata section
                if (toCheckString[sectionindex + 1] == '!')
                {
                    sectionEndIndex = toCheckString.IndexOf("]]>") + 3; // we have to add the string length
                    if (sectionEndIndex == -1)
                        return false; // this means our cdata section is malformed
                    string CDATASection = toCheckString.Substring(new Range { StartIndex = sectionindex, EndIndex = sectionEndIndex });
                    if (!ReadCharacterDataSection(CDATASection))
                        return false;
                    toCheckString = toCheckString.RemoveFirst(CDATASection);
                }
                else
                {
                    sectionEndIndex = toCheckString.IndexOf('>') + 1; // we have to add 1 since it's the null based index form the start of the string
                    if (sectionEndIndex == -1)
                        return false; // means the child element is malformed
                    string tag = toCheckString.Substring(new Range { StartIndex = sectionindex, EndIndex = sectionEndIndex }); // we get the tag

                    Func<string, bool> predicate;
                    switch (tag)
                    {
                        case var _ when tag.EndsWith("/>"): // check if it is an empty tag
                            predicate = ReadEmptyElementTag;
                            break;
                        case var _ when tag.StartsWith("</"): // check if it is an ending tag
                            predicate = ReadEndTag;
                            break;
                        default: // otherwise it's a starting tag, validate it as such
                            predicate = ReadStartTag;
                            break;
                    }
                    if (!predicate(tag))
                        return false;
                    toCheckString = toCheckString.RemoveFirst(tag);
                }
            }
            return true;
        }

        // this one was a bitch to implement
        // '<' Name (S Attribute)* S? '/>'
        private bool ReadEmptyElementTag(string toCheckString) =>
            ReadTag(toCheckString, "/>");

        // '<' Name (S Attribute)* S? <close>
        private bool ReadTag(string toCheckString, string close)
        {
            if (!AssertContainedAndUpdate(toCheckString, 4, "<", close, out string body))
                return false;
            // todo; dont use check reference
            string[] tokens = body.Split(Constants.whiteSpace, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 1)
                return false;
            string name = tokens[0];
            if (!GenericXMLLookupTable["name"](name))
                return false;
            // get all attributes using body
            body = body.RemoveFirst(name).TrimEnd(Constants.whiteSpace);
            // first remove the trailing space
            // then we can maybe loop while the string is not empty
            // if something is not true
            // return false
            // otherwise return true
            while (true)
            {
                // trim whitespace
                body = body.TrimStart(Constants.whiteSpace);
                // get the position at where we'll split
                int equalsIndex = body.IndexOf('=');
                if (equalsIndex == -1)
                    return true;
                // get the name of the attribute and validate it
                string attributeName = body.Substring(0, equalsIndex).TrimEnd(Constants.whiteSpace);
                if (!GenericXMLLookupTable["name"](attributeName))
                    return false;
                // update body
                body = body.Substring(equalsIndex + 1).TrimStart(Constants.whiteSpace);
                // get the value of the attribute and validate it
                char quote = body.First();
                int valueLen = body.Substring(1).IndexOf(quote) + 2;
                string value = body.Substring(0, valueLen);
                if (!GenericXMLLookupTable["AttValue"](value))
                    return false;
                // update body again
                body = body.RemoveFirst(value);
            }
        }
    }
}
