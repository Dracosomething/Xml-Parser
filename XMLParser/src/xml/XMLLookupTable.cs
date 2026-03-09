using System;
using System.Collections.Generic;
using System.Text;
using XmlParser.src;
using XmlParser.src.xml;

namespace XMLParser.src.xml
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
        private GenericXMLLookupTable GenericXMLLookupTable = new();
        private LookupTable<string, Func<string, bool>> table = new(
            new Dictionary<string, Func<string, bool>>
            {

            }
        );
    }
}
