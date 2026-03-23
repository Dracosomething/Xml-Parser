using System.Text.RegularExpressions;
using XmlParser.src.extentions.@string;

namespace XmlParser.src.xml
{
    /// <summary>
    /// Lookup table for every xml related expresion
    /// [14]   CharData            ::=       [^<&]* - ([^<&]* ']]>' [^<&]*)
    /// [18]   CDSect              ::=       CDStart CData CDEnd
    /// [19]   CDStart             ::=       '<![CDATA['
    /// [20]   CData               ::=       (Char* - (Char* ']]>' Char*))
    /// [21]   CDEnd               ::=       ']]>'
    /// [22]   prolog              ::=       XMLDecl? Misc* (doctypedecl Misc*)?
    /// [23]   XMLDecl             ::=       '<?xml' VersionInfo EncodingDecl? SDDecl? S? '?>'
    /// [28]   doctypedecl         ::=       '<!DOCTYPE' S Name (S ExternalID)? S? ('[' intSubset ']' S?)? '>'    [VC: Root Element Type]
    /// [32]   SDDecl              ::=       S 'standalone' Eq (("'" ('yes' | 'no') "'") | ('"' ('yes' | 'no') '"'))     [VC: Standalone Document Declaration]
    /// [39]   element             ::=       EmptyElemTag | STag content ETag 
    /// [40]   STag                ::=       '<' Name (S Attribute)* S? '>'    [WFC: Unique Att Spec]
    /// [41]   Attribute           ::=       Name Eq AttValue     [VC: Attribute Value Type]
    /// [42]   ETag                ::=       '</' Name S? '>'
    /// [43]   content             ::=       CharData? ((element | Reference | CDSect | PI | Comment) CharData?)*
    /// [44]   EmptyElemTag        ::=       '<' Name (S Attribute)* S? '/>'    [WFC: Unique Att Spec]
    /// </summary>
    internal class XMLLookupTable : BaseLookupTableFunctions
    {
        private GenericXMLLookupTable GenericXMLLookupTable;
        private LookupTable<string, Func<string, bool>> table;

        public XMLLookupTable(bool isXML10)
        {
            GenericXMLLookupTable = new(isXML10);
            table = new(
                new Dictionary<string, Func<string, bool>>
                {
                    { "chardata"        , ReadCharacterData                 },
                    { "cdsect"          , ReadCharacterDataSection          },
                    { "prolog"          , ReadProlog                        },
                    { "xmldecl"         , ReadXMLDeclreration               },
                    { "sddecl"          , ReadstandaloneDocumentDecleration },
                    { "element"         , ReadElement                       },
                    { "stag"            , ReadStartTag                      },
                    { "attribute"       , ReadAttribute                     },
                    { "etag"            , ReadEndTag                        },
                    { "content"         , ReadContent                       },
                    { "emptyelemtag"    , ReadEmptyElementTag               }
                }
            );
        }

        public Func<string, bool> this[string str] => table[str.ToLower()];

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
        // needs fixing
        private bool ReadEndTag(string toCheckString) =>
            AssertContainedAndUpdate(toCheckString, 4, "</", ">", out string data) &&
            CheckReference(data, GenericXMLLookupTable["Name"], 1, out data) &&
            ReadOptional(data, 0, GenericXMLLookupTable["S"], out data) &&
            data == string.Empty;

        // CharData? ((element | Reference | CDSect | PI | Comment) CharData?)*
        private bool ReadContent(string toCheckString)
        {
            if (!AssertMinLength(toCheckString, 3))
                return false;
            ReadOptional(toCheckString, 0, ReadCharacterData, out string body);
            // ((element | Reference | CDSect | PI | Comment) CharData?)*
            // (element | Reference | CDSect | PI | Comment)
            // this is the group
            for (int i = 0; i < body.Length; i++)
            {
                char c = body[i];
                if (c == '<' && (body[i + 1] != '-' || body[i + 1] != '!'))
                {
                    // its an element
                    // read and update
                    if (!CheckReference(body, ReadElement, 4, out body))
                        return false;
                }
                else
                {
                    bool func(string str) =>
                        GenericXMLLookupTable["Reference"](str) ||
                        ReadCharacterDataSection(str) ||
                        GenericXMLLookupTable["PI"](str) ||
                        GenericXMLLookupTable["Comment"](str);
                    if (!CheckReference(body, func, 3, out body))
                        return false;
                }
                ReadOptional(toCheckString, 0, ReadCharacterData, out body);
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
