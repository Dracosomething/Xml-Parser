using System;
using System.Collections.Generic;
using System.Text;
using XmlParser.src;
using XmlParser.src.xml;

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
    internal class XMLLookupTable
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
                    { "doctypedecl"     , ReadDocumentTypeDecleration       },
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
            Constants.RegexMatch(toCheckString, "[^<&]*") && !toCheckString.Contains("]]>");

        private bool ReadCharacterDataSection(string toCheckString)
        {
            // '<![CDATA[' (Char* - (Char* ']]>' Char*)) ']]>'
            if (!)
            if (toCheckString.Length < 13)
                return false;
            if (!toCheckString.StartsWith("<![CDATA[") && !toCheckString.EndsWith("]]>"))
                return false;

            int bodyStartIndex = "<![CDATA[".Length - 1;
            int bodyEndIndex = toCheckString.Length - 1 - 3; // remove 1 cuz length will always be 1 longer then index and remove 3 since "]]>" is 3 characters long
            int bodyLength = bodyEndIndex - bodyStartIndex;

            string body = toCheckString.Substring(bodyStartIndex, bodyLength);
            var charExpression = GenericXMLLookupTable["Char"];
            return body.All(c => charExpression(c.ToString())) && !body.Contains("]]>");
        }

        private bool ReadProlog(string toCheckString)
        {
            // XMLDecl? Misc* (doctypedecl Misc*)?
        }

        private bool ReadXMLDeclreration(string toCheckString)
        {
            // '<?xml' VersionInfo EncodingDecl? SDDecl? S? '?>'
            if (!toCheckString.StartsWith("<?xml") || !toCheckString.EndsWith("?>"))
                return false;

            int bodyStartIndex = "<?xml".Length - 1;
            int bodyEndIndex = toCheckString.Length - 1 - 2; // remove 1 cuz the length is always 1 longer then the index and remove 2 cuz "?>".Length is 2 
            int bodyLength = bodyEndIndex - bodyStartIndex;

            string leftover = toCheckString.Substring(bodyStartIndex, bodyLength);
            var versionInfoExpression = GenericXMLLookupTable["VersionInfo"];
            var match = leftover.FirstMatch(versionInfoExpression);
            
        }
    }
}
