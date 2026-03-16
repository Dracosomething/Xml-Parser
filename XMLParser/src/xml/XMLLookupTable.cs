using XmlParser.src.extentions;
using XMLParser.src;

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
                    { "CharData"        , ReadCharacterData                 },
                    { "CDSect"          , ReadCharacterDataSection          },
                    { "prolog"          , ReadProlog                        },
                    { "XMLDecleration"  , ReadXMLDeclreration               },
                    { "SDDecl"          , ReadstandaloneDocumentDecleration },
                    { "element"         , ReadElement                       },
                    { "STag"            , ReadStartTag                      },
                    { "Attribute"       , ReadAttribute                     },
                    { "ETag"            , ReadEndTag                        },
                    { "content"         , ReadContent                       },
                    { "EmptyElemTag"    , ReadEmptyElementTag               }
                }
            );
        }

        public Func<string, bool> this[string str] => table[str];

        //[^<&]* - ([^<&]* ']]>' [^<&]*)
        private bool ReadCharacterData(string toCheckString) =>
            Utils.RegexMatch(toCheckString, "[^<&]*") && !toCheckString.Contains("]]>");

        // '<![CDATA[' (Char* - (Char* ']]>' Char*)) ']]>'
        private bool ReadCharacterDataSection(string toCheckString) =>
            AssertContainedAndUpdate(toCheckString, 13, "<![CDATA[", "]]>", out string body) &&
            (body.AllString(GenericXMLLookupTable["Char"]) && !body.Contains("]]>"));

        // XMLDecl? Misc* (doctypedecl Misc*)?
        // the (doctypedecl Misc*)? has not been implemented yet
        private bool ReadProlog(string toCheckString) =>
            ReadOptional(toCheckString, 0, ReadXMLDeclreration, out string extra) &&
            (extra == string.Empty || extra.AllString(GenericXMLLookupTable["Misc"]));

        // '<?xml' VersionInfo EncodingDecl? SDDecl? S? '?>'
        private bool ReadXMLDeclreration(string toCheckString) =>
            // min length = '<?xml'.Length + VersionInfo->minLength + '?>'.Length = 5 + 14 + 2 = 21
            AssertContainedAndUpdate(toCheckString, 21, "<?xml", "?>", out string body) &&
            // VersionInfo
            CheckReference(body, GenericXMLLookupTable["VersionInfo"],
                match => match.StartIndex == 0 && match.Length >= 14,
                match => true,
                out body) &&
            // EncodingDecl?
            ReadOptional(body, 0, GenericXMLLookupTable["EncodingDecl"], out body) &&
            // SDDecl?
            ReadOptional(body, 0, ReadstandaloneDocumentDecleration, out body) &&
            // S?
            ReadOptional(body, 0, GenericXMLLookupTable["S"], out body) &&
            body == string.Empty; // string must be empty after this.

        // S 'standalone' Eq (("'" ('yes' | 'no') "'") | ('"' ('yes' | 'no') '"'))
        private bool ReadstandaloneDocumentDecleration(string toCheckString) =>
            GenericXMLLookupTable.ReadDeclaration(toCheckString, "standalone", 2, (str) => str == "yes" || str == "no");

        // EmptyElemTag | STag content ETag 
        private bool ReadElement(string toCheckString) =>
            ReadEmptyElementTag(toCheckString) ||
                (CheckReference(toCheckString, ReadStartTag, 4, out toCheckString) &&
                 CheckReference(toCheckString, ReadContent, 3, out toCheckString) &&
                 CheckReference(toCheckString, ReadEndTag, 4, out toCheckString));

        // '<' Name (S Attribute)* S? '>'
        private bool ReadStartTag(string toCheckString) =>
            ReadTag(toCheckString, ">");

        // Name Eq AttValue
        private bool ReadAttribute(string toCheckString) =>
            AssertMinLength(toCheckString, 4) &&
            CheckAllRefernces(toCheckString, [
                GenericXMLLookupTable["Name"],
                GenericXMLLookupTable["Eq"],
                GenericXMLLookupTable["AttValye"]
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
                    Func<string, bool> func = (str) =>
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

        private bool ReadTag(string toCheckString, string close)
        {
            if (!AssertContainedAndUpdate(toCheckString, 4, "<", close, out string body))
                return false;
            if (!CheckReference(body, GenericXMLLookupTable["Name"], match => match.StartIndex != 0,
                match => true,
                out body))
                return false;
            // read space and check if there is still space left in the string
            ReadOptional(body, 0, GenericXMLLookupTable["S"], out string tmp);
            if (tmp == string.Empty)
                return true;
            // first remove the trailing space
            // then we can maybe loop while the string is not empty
            // if something is not true
            // return false
            // otherwise return true
            body = body.TrimEnd(Constants.whiteSpace);
            while (body != string.Empty)
            {
                if (!CheckAllRefernces(body, [GenericXMLLookupTable["S"], ReadAttribute], [1, 1], out body))
                    return false;
            }
            return toCheckString.EndsWith(GenericXMLLookupTable["S"]);
        }
    }
}
