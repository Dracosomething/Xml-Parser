namespace XmlParser.src.xml
{
    /// <summary>
    /// Lookup table for all the things that can occur in both the DTD and XML files.
    /// 
    /// [2]    Char                ::=       #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]    /* any Unicode character, excluding the surrogate blocks, FFFE, and FFFF. */
    /// [3]    S                   ::=       (#x20 | #x9 | #xD | #xA)+
    /// [4]    NameStartChar       ::=       ":" | [A-Z] | "_" | [a-z] | [#xC0-#xD6] | [#xD8-#xF6] | [#xF8-#x2FF] | [#x370-#x37D] | [#x37F-#x1FFF] | [#x200C-#x200D] | [#x2070-#x218F] | [#x2C00-#x2FEF] | [#x3001-#xD7FF] | [#xF900-#xFDCF] | [#xFDF0-#xFFFD] | [#x10000-#xEFFFF]
    /// [4a]   NameChar            ::=       NameStartChar | "-" | "." | [0 - 9] | #xB7 | [#x0300-#x036F] | [#x203F-#x2040]
    /// [5]    Name                ::=       NameStartChar (NameChar)*  
    /// [6]    Names               ::=       Name (#x20 Name)*
    /// [10]   AttValue            ::=       '"' ([^<&"] | Reference)* '"' |  "'" ([^<&'] | Reference)* "'"
    /// [11]   SystemLiteral       ::=       ('"' [^"]* '"') | ("'" [^']* "'")
    /// [12]   PubidLiteral        ::=       '"' PubidChar* '"' | "'" (PubidChar - "'")* "'"
    /// [13]   PubidChar           ::=       #x20 | #xD | #xA | [a-zA-Z0-9] | [-'()+,./:=?;!*#@$_%]
    /// [15]   Comment             ::=       '<!--' ((Char - '-') | ('-' (Char - '-')))* '-->'
    /// [16]   PI                  ::=       '<?' PITarget (S (Char* - (Char* '?>' Char*)))? '?>'
    /// [17]   PITarget            ::=       Name - (('X' | 'x') ('M' | 'm') ('L' | 'l'))
    /// [24]   VersionInfo         ::=       S 'version' Eq ("'" VersionNum "'" | '"' VersionNum '"')
    /// [25]   Eq                  ::=       S? '=' S?
    /// [26]   VersionNum          ::=       '1.' [0-9]+
    /// [27]   Misc                ::=       Comment | PI | S
    /// [66]   CharRef             ::=       '&#' [0-9]+ ';' | '&#x' [0-9a-fA-F]+ ';'    [WFC: Legal Character]
    /// [67]   Reference           ::=       EntityRef | CharRef
    /// [68]   EntityRef           ::=       '&' Name ';'    
    /// [69]   PEReference         ::=       '%' Name ';'    
    /// [80]   EncodingDecl        ::=       S 'encoding' Eq ('"' EncName '"' | "'" EncName "'" )
    /// [81]   EncName             ::=       [A-Za-z] ([A-Za-z0-9._] | '-')*    /* Encoding name contains only Latin characters */
    /// 
    /// xml 11
    /// [2]   Char              ::= [#x1-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF] /* any Unicode character, excluding the surrogate blocks, FFFE, and FFFF. */
    /// [2a]  RestrictedChar    ::= [#x1-#x8] | [#xB-#xC] | [#xE-#x1F] | [#x7F-#x84] | [#x86-#x9F]
    /// </summary>
    internal partial class GenericXMLLookupTable : BaseLookupTableFunctions
    {
        private bool isXML10;
        private LookupTable<string, Func<string, bool>> table;

        public GenericXMLLookupTable(bool isXML10)
        {
            table = new(
                new Dictionary<string, Func<string, bool>>()
                {
                    { "Char"            , ValidateCharacter                 },
                    { "Space"           , IsSpace                           },
                    { "RestrictedChar"  , IsRestrictedCharacter             },
                    { "NameStartChar"   , ReadNameStartCharacter            },
                    { "NameChar"        , ReadNameCharacter                 },
                    { "Name"            , ReadName                          },
                    { "Names"           , ReadNames                         },
                    { "AttValue"        , ReadAttributeValue                },
                    { "SystemLiteral"   , ReadSystemLiteral                 },
                    { "PubidLiteral"    , ReadPubidLiteral                  },
                    { "Comment"         , ReadComment                       },
                    { "PI"              , ReadProcessingInstruction         },
                    { "VersionInfo"     , ReadVersionInfo                   },
                    { "Eq"              , ReadEquals                        },
                    { "VersionNum"      , ReadVersionNum                    },
                    { "Misc"            , ReadMisc                          },
                    { "CharRef"         , ReadCharacterReference            },
                    { "Reference"       , ReadReference                     },
                    { "EntityRef"       , ReadEntityReference               },
                    { "PEReference"     , ReadParsedEntityReference         },
                    { "EncodingDecl"    , ReadEncodingDecleration           },
                    { "EncName"         , ReadEncodingName                  },
                }
           );
            this.isXML10 = isXML10;
        }

        public Func<string, bool> this[string str] => table[str];
    }
}
