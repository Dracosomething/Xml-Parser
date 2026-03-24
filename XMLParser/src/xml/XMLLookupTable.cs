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
    internal partial class XMLLookupTable : BaseLookupTableFunctions
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
    }
}
