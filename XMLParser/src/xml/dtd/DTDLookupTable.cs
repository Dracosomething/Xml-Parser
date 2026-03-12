using XmlParser.src;
using XmlParser.src.xml;
using Range = XmlParser.src.Range;

namespace XMLParser.src.xml.dtd
{
    /// <summary>
    /// Lookup table for every DTD related expression
    /// [7]    Nmtoken             ::=       (NameChar)+
    /// [8]    Nmtokens            ::=       Nmtoken (#x20 Nmtoken)*
    /// [9]    EntityValue         ::=       '"' ([^%&"] | PEReference | Reference)* '"' |  "'" ([^%&'] | PEReference | Reference)* "'"
    /// [28a]  DeclSep             ::=       PEReference | S     [WFC: PE Between Declarations]
    /// [28b]  intSubset           ::=       (markupdecl | DeclSep)*
    /// [29]   markupdecl          ::=       elementdecl | AttlistDecl | EntityDecl | NotationDecl | PI | Comment     [VC: Proper Declaration/PE Nesting]
    /// [30]   extSubset           ::=       TextDecl? extSubsetDecl
    /// [31]   extSubsetDecl       ::=       (markupdecl | conditionalSect | DeclSep)*
    /// [45]   elementdecl         ::=       '<!ELEMENT' S Name S contentspec S? '>'    [VC: Unique Element Type Declaration]
    /// [46]   contentspec         ::=       'EMPTY' | 'ANY' | Mixed | children
    /// [47]   children            ::=       (choice | seq) ('?' | '*' | '+')?
    /// [48]   cp                  ::=       (Name | choice | seq) ('?' | '*' | '+')?
    /// [49]   choice              ::=       '(' S? cp ( S? '|' S? cp )+ S? ')'    [VC: Proper Group/PE Nesting]
    /// [50]   seq                 ::=       '(' S? cp ( S? ',' S? cp )* S? ')'    [VC: Proper Group/PE Nesting]
    /// [51]   Mixed               ::=       '(' S? '#PCDATA' (S? '|' S? Name)* S? ')*' | '(' S? '#PCDATA' S? ')'
    /// [52]   AttlistDecl         ::=       '<!ATTLIST' S Name AttDef* S? '>'
    /// [53]   AttDef              ::=       S Name S AttType S DefaultDecl
    /// [54]   AttType             ::=       StringType | TokenizedType | EnumeratedType
    /// [55]   StringType          ::=       'CDATA'
    /// [56]   TokenizedType       ::=       'ID' | 'IDREF' | 'IDREFS' | 'ENTITY' | 'ENTITIES' | 'NMTOKEN' | 'NMTOKENS'
    /// [57]   EnumeratedType      ::=       NotationType | Enumeration
    /// [58]   NotationType        ::=       'NOTATION' S '(' S? Name (S? '|' S? Name)* S? ')'     [VC: Notation Attributes]
    /// [59]   Enumeration         ::=       '(' S? Nmtoken (S? '|' S? Nmtoken)* S? ')'    [VC: Enumeration]
    /// [60]   DefaultDecl         ::=       '#REQUIRED' | '#IMPLIED' | (('#FIXED' S)? AttValue)
    /// [61]   conditionalSect     ::=       includeSect | ignoreSect
    /// [62]   includeSect         ::=       '<![' S? 'INCLUDE' S? '[' extSubsetDecl ']]>'     [VC: Proper Conditional Section/PE Nesting]
    /// [63]   ignoreSect          ::=       '<![' S? 'IGNORE' S? '[' ignoreSectContents* ']]>'    [VC: Proper Conditional Section/PE Nesting]
    /// [64]   ignoreSectContents  ::=       Ignore ('<![' ignoreSectContents ']]>' Ignore)*
    /// [65]   Ignore              ::=       Char* - (Char* ('<![' | ']]>') Char*)
    /// [70]   EntityDecl          ::=       GEDecl | PEDecl
    /// [71]   GEDecl              ::=       '<!ENTITY' S Name S EntityDef S? '>'
    /// [72]   PEDecl              ::=       '<!ENTITY' S '%' S Name S PEDef S? '>'
    /// [73]   EntityDef           ::=       EntityValue | (ExternalID NDataDecl?)
    /// [74]   PEDef               ::=       EntityValue | ExternalID
    /// [75]   ExternalID          ::=       'SYSTEM' S SystemLiteral | 'PUBLIC' S PubidLiteral S SystemLiteral
    /// [76]   NDataDecl           ::=       S 'NDATA' S Name     [VC: Notation Declared]
    /// [77]   TextDecl            ::=       '<?xml' VersionInfo? EncodingDecl S? '?>'
    /// [78]   extParsedEnt        ::=       TextDecl? content
    /// [82]   NotationDecl        ::=       '<!NOTATION' S Name S (ExternalID | PublicID) S? '>'    [VC: Unique Notation Name]
    /// [83]   PublicID            ::=       'PUBLIC' S PubidLiteral
    /// </summary>
    internal class DTDLookupTable : BaseLookupTableFunctions
    {
        private GenericXMLLookupTable GenericXMLLookupTable;
        private LookupTable<string, Func<string, bool>> table;

        public DTDLookupTable(bool isXML10)
        {
            GenericXMLLookupTable = new(isXML10);
            table = new(
                new Dictionary<string, Func<string, bool>>
                {
                    { "Nmtoken"             , ReadNameToken                     },
                    { "Nmtokens"            , ReadNameTokens                    },
                    { "EntityValue"         , ReadEntityValue                   },
                    { "DeclSep"             , ReadDeclerationSeperation         },
                    { "intSubset"           , ReadInternalSubset                },
                    { "markupdecl"          , ReadMarkupDeclaration             },
                    { "extSubset"           , ReadExternalSubset                },
                    { "extSubsetDecl"       , ReadExternalSubsetDecleration     },
                    { "elementDecl"         , ReadElementDecleration            },
                    { "contentSpec"         , ReadContentSpecification          },
                    { "children"            , ReadChildren                      },
                    { "cp"                  , ReadContentParticles              },
                    { "choice"              , ReadChoice                        },
                    { "seq"                 , ReadSequence                      },
                    { "Mixed"               , ReadMixed                         },
                    { "AttlistDecl"         , ReadAttributeListDecleration      },
                    { "AttDef"              , ReadAttributeDefinition           },
                    { "AttType"             , ReadAttributeType                 },
                    { "StringType"          , ReadStringType                    },
                    { "TokenizedType"       , ReadTokenizedType                 },
                    { "NotationType"        , ReadNotationType                  },
                    { "EnumerationType"     , ReadEnumerationType               },
                    { "DefaultDecl"         , ReadDefaultDecleration            },
                    { "conditionalSect"     , ReadConditionalSection            },
                    { "includeSect"         , ReadIncludeSection                },
                    { "ignoreSect"          , ReadIgnoreSection                 },
                    { "ignoreSectContents"  , ReadIgnoreSectionContents         },
                    { "Ignore"              , ReadIgnore                        },
                    { "EntityDecl"          , ReadEntityDecleration             },
                    { "GEDecl"              , ReadGeneralEntityDecleration      },
                    { "PEDecl"              , ReadParameterEntityDecleration    },
                    { "EntityDef"           , ReadEntityDefinition              },
                    { "PEDef"               , ReadParameterEntityDefinition     },
                    { "ExternalID"          , ReadExternalID                    },
                    { "NDataDecl"           , ReadNotationDataDecleration       },
                    { "TextDecl"            , ReadTextDecleration               },
                    { "extParsedEnt"        , ReadExternalParsedEntity          },
                    { "NotationDecl"        , ReadNotationDecleration           },
                    { "PublicID"            , ReadPublicID                      }
                }
            );
        }

        public Func<string, bool> this[string key] => table[key];

        // (NameChar)+
        private bool ReadNameToken(string toCheckString) =>
            AssertMinLength(toCheckString, 1) &&
            toCheckString.AllString(GenericXMLLookupTable["NameChar"]);

        // Nmtoken (#x20 Nmtoken)*
        private bool ReadNameTokens(string toCheckString) => 
            ReadMultipleTokens(toCheckString, ' ', ReadNameToken);

        // '"' ([^%&"] | PEReference | Reference)* '"' |  "'" ([^%&'] | PEReference | Reference)* "'"
        private bool ReadEntityValue(string toCheckString) => 
            ReadQuoted(toCheckString, quote => $"[^{quote}]");

        // PEReference | S
        private bool ReadDeclerationSeperation(string toCheckstring) =>
            GenericXMLLookupTable["PEReference"](toCheckstring) || GenericXMLLookupTable["S"](toCheckstring);

        // (markupdecl | DeclSep)*
        private bool ReadInternalSubset(string toCheckString);

        // elementdecl | AttlistDecl | EntityDecl | NotationDecl | PI | Comment
        private bool ReadMarkupDeclaration(string toCheckString);

        // TextDecl? extSubsetDecl
        private bool ReadExternalSubset(string toCheckString);

        // (markupdecl | conditionalSect | DeclSep)*
        private bool ReadExternalSubsetDecleration(string toCheckString);

        // '<!ELEMENT' S Name S contentspec S? '>'
        private bool ReadElementDecleration(string toCheckString) => 
            ReadDecleration(toCheckString, "ELEMENT", 3, ReadContentSpecification);

        // 'EMPTY' | 'ANY' | Mixed | children
        private bool ReadContentSpecification(string toCheckString) =>
            AssertMinLength(toCheckString, 3) &&
            toCheckString == "EMPTY" || toCheckString == "ANY" || ReadMixed(toCheckString) || ReadChildren(toCheckString);

        // (choice | seq) ('?' | '*' | '+')?
        private bool ReadChildren(string toCheckString) =>
            AssertMinLength(toCheckString, 3) &&
            CheckReference(toCheckString, (str) => ReadChoice(str) || ReadSequence(str),
                match => match.StartIndex == 0,
                match => true,
                out string body) &&
            ReadOptional(body, 0, (str) => str == "?" || str == "*" || str == "+", out body) &&
            body == string.Empty;

        // (Name | choice | seq) ('?' | '*' | '+')?
        private bool ReadContentParticles(string toCheckString)
        {
            /*
             * if string starts with '(' and the last ')' is eather the last or second to last character
             * read it as a choice or sequence
             * otherwise read it as name
             * then read the ('?' | '*' | '+')? if present
             */
            ReadOptional(toCheckString, toCheckString.EndIndex,
               (str) => str == "?" || str == "*" || str == "+", out string expression);
            int lastIndexOfClosingBracket = expression.LastIndexOf(")");
            if (expression.StartsWith("(") && (lastIndexOfClosingBracket == expression.EndIndex))
                // possible improvement would be to first check if it is a choice or sequence and depending on that specificlly do the check to save on time
                return ReadChoice(expression) || ReadSequence(expression);
            else
                return GenericXMLLookupTable["Name"](expression);
        }


        // '(' S? cp ( S? '|' S? cp )+ S? ')'
        private bool ReadChoice(string toCheckString)
        {
            if (!AssertContainedAndUpdate(toCheckString, 5, "(", ")", out string body))
                return false;
            ReadOptional(body, 0, GenericXMLLookupTable["S"], out body);
            
                
        }

        // '(' S? cp ( S? ',' S? cp )* S? ')'
        private bool ReadSequence(string toCheckString);

        // '(' S? '#PCDATA' (S? '|' S? Name)* S? ')*' | '(' S? '#PCDATA' S? ')'
        private bool ReadMixed(string toCheckString);

        // // '<!<name>' S Name S contentspec S? '>'
        private bool ReadDecleration(string toCheckString, string name, int minBodyLen, Func<string, bool> bodyCheck) =>
            AssertContainedAndUpdate(toCheckString, (name.Length + minBodyLen + 6), $"<!{name}", ">", out string body) &&
            // S
            CheckReference(body, GenericXMLLookupTable["S"],
                match => match.StartIndex == 0,
                match => true,
                out body) &&
            // Name
            CheckReference(body, GenericXMLLookupTable["Name"],
                match => match.StartIndex == 0,
                match => true,
                out body) &&
            // S
            CheckReference(body, GenericXMLLookupTable["S"],
                match => match.StartIndex == 0,
                match => true,
                out body) &&
            // contentspec
            CheckReference(body, bodyCheck,
                match => match.StartIndex == 0,
                match => true,
                out body) &&
            body.IsWhiteSpace() &&
            ReadOptional(body, 0, GenericXMLLookupTable["S"], out body) &&
            body == string.Empty;
    }
}
