using System;
using System.Collections.Generic;
using System.Text;
using XmlParser.src;
using XmlParser.src.xml;

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
    /// [31]   extSubsetDecl       ::=       ( markupdecl | conditionalSect | DeclSep)*
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
    internal class DTDLookupTable
    {
        private GenericXMLLookupTable GenericXMLLookupTable = new();
        private LookupTable<string, Func<string, bool>> table = new(
            new Dictionary<string, Func<string, bool>>
            {

            }
        );
    }
}
