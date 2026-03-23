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
                    { "xmldecleration"  , ReadXMLDeclreration               },
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
        // fix this by getting the first instance of '>' and then checking if from start to that pos is equal to a start tag
        // then get the last index of  '<' and substring it there to check if it is equal to an end tag
        // lastly check if everything inbetween is equal to content
        /*(CheckReference(toCheckString, ReadStartTag, 4, out toCheckString) &&
         CheckReference(toCheckString, ReadContent, -1, out toCheckString) &&
         CheckReference(toCheckString, ReadEndTag, 4, out toCheckString));*/
        Return(() =>
            {
                int endPosStartingTag = toCheckString.IndexOf('>');
                if (endPosStartingTag == -1)
                    return false;
                string startTag = toCheckString[..endPosStartingTag];
                if (!ReadStartTag(startTag))
                    return false;
                string contentAndEndTag = toCheckString[endPosStartingTag..];
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
        private bool ReadAttribute(string toCheckString) =>
            AssertMinLength(toCheckString, 4) &&
            CheckAllRefernces(toCheckString, [
                GenericXMLLookupTable["Name"],
                GenericXMLLookupTable["Eq"],
                GenericXMLLookupTable["AttValue"]
            ], [1, 1, 2], out toCheckString) &&
            toCheckString == string.Empty;

        // '</' Name S? '>'
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
            if (!CheckReference(body, GenericXMLLookupTable["Name"], match => match.StartIndex != 0,
                match => true,
                out body))
                return false;
            // read space and check if there is still space left in the string
            string tmp = body.TrimStart(Constants.whiteSpace);
            if (tmp == string.Empty)
                return true;
            // first remove the trailing space
            // then we can maybe loop while the string is not empty
            // if something is not true
            // return false
            // otherwise return true
            body = body.TrimEnd(Constants.whiteSpace);
            string[] attributes = body.Split(Constants.whiteSpace, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < attributes.Length; i++)
            {
                string attribute = attributes[i];
                if (!ReadAttribute(attribute))
                    return false;
            }
            return toCheckString.EndsWith(GenericXMLLookupTable["S"]);
        }
    }
}
