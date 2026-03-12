using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

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
    internal class GenericXMLLookupTable : BaseLookupTableFunctions
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

        // #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
        private bool ValidateCharacterXML10(string toCheckString) =>
            (Constants.RegexMatch(toCheckString, "\\t|\\n|\\r|[\\s-\\uD7FF]|[\\uE000-\\uFFFD]") ||
            (toCheckString[0] >= 0x10000 && toCheckString[0] <= 0x10FFFF));

        // 	[#x1-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
        private bool ValidateCharacterXML11(string toCheckString) =>
            (Constants.RegexMatch(toCheckString, "[\\x01-\\uD7FF]|[\\uE000-\\uFFFD]") || (toCheckString[0] >= 0x10000 && toCheckString[0] <= 0x10FFFF));

        private bool ValidateCharacter(string toCheckString) =>
            AssertCharacter(toCheckString) &&
            isXML10 ? ValidateCharacterXML10(toCheckString) : ValidateCharacterXML11(toCheckString);

        // (#x20 | #x9 | #xD | #xA)+
        private bool IsSpace(string toCheckString) => 
            AssertMinLength(toCheckString, 1) &&
            Constants.RegexMatch(toCheckString, "(\\s|\\t|\\r|\\n)+");

        // [#x1-#x8] | [#xB-#xC] | [#xE-#x1F] | [#x7F-#x84] | [#x86-#x9F]
        private bool IsRestrictedCharacter(string toCheckString) =>
            AssertCharacter(toCheckString) && 
            Constants.RegexMatch(toCheckString, "[\\x01-\\x08]|[\\x0B-\\x0C]|[\\x0E-\\x1F]|[\\x7F-\\x84]|[\\x86-\\x9F]");

        // ":" | [A-Z] | "_" | [a-z] | [#xC0-#xD6] | [#xD8-#xF6] | [#xF8-#x2FF] | [#x370-#x37D] | [#x37F-#x1FFF] | [#x200C-#x200D] |
        // [#x2070-#x218F] | [#x2C00-#x2FEF] | [#x3001-#xD7FF] | [#xF900-#xFDCF] | [#xFDF0-#xFFFD] | [#x10000-#xEFFFF]
        private bool ReadNameStartCharacter(string toCheckString) =>
            AssertCharacter(toCheckString) &&
            (Constants.RegexMatch(toCheckString, ":|[A-Z]|_|[a-z]|[À-Ö]|[Ø-ö]|[ø-˿]|[Ͱ-ͽ]|[Ϳ-\\u1FFF]|[\\u200C-\\u200D]|[⁰-\\u218F]|[Ⰰ-\\u2fef]|" +
                "[、-\\uD7FF]|[豈-\\uFDCF]|[\\uFDF0-\\uFFFD]") || (toCheckString[0] >= 0x10000 && toCheckString[0] <= 0xEFFFF));

        //  NameStartChar | "-" | "." | [0-9] | #xB7 | [#x0300-#x036F] | [#x203F-#x2040]
        private bool ReadNameCharacter(string toCheckString) =>
            AssertCharacter(toCheckString) && 
            (ReadNameStartCharacter(toCheckString) || // unicode 203f to unicode 2040
             Constants.RegexMatch(toCheckString, "-|\\.|[0-9]|·|[\\u0300-\\u036F]|[‿-⁀]"));

        // 	NameStartChar (NameChar)*
        private bool ReadName(string toCheckString) =>
            AssertMinLength(toCheckString, 1) && (
                toCheckString.Length > 1 ? (
                    toCheckString.StartsWith(ReadNameStartCharacter) && 
                    toCheckString.Substring(1).All(c => ReadNameCharacter(c.ToString()))
                ) : toCheckString.StartsWith(ReadNameStartCharacter)
            );

        private bool ReadNames(string toCheckString)
        {
            // Name (#x20 Name)*
            if (!AssertMinLength(toCheckString, 1))
                return false;
            string[] items = toCheckString.Split(' '); // split all the names;
            return items.Length > 1 && items.All(ReadName);
        }

        // '"' ([^<&"] | Reference)* '"' | "'" ([^<&'] | Reference)* "'"
        private bool ReadAttributeValue(string toCheckString) => 
            ReadQuoted(toCheckString, (quote) => $"[^<{quote}]*");

        // ('"' [^"]* '"') | ("'" [^']* "'")
        private bool ReadSystemLiteral(string toCheckString) => 
            ReadQuoted(toCheckString, (quote) => $"[^{quote}]*");

        // '"' PubidChar* '"' | "'" (PubidChar - "'")* "'"
        private bool ReadPubidLiteral(string toCheckString) => 
            ReadQuoted(toCheckString, (quote) => $"\\s|\\r|\\n|[a-zA-Z0-9]|[-{(quote == '"' ? '\'' : "")}()+,./:=?;!*#@$_%]");
        
        private bool ReadComment(string toCheckString)
        {
            // '<!--' ((Char - '-') | ('-' (Char - '-')))* '-->'
            if (!AssertContainedAndUpdate(toCheckString, 7, "<!--", "-->", out string commentBody))
                return false;
            for (int i = 0; i < commentBody.Length; i++)
            {
                char c = commentBody[i];
                if (!ValidateCharacter(c.ToString()))
                    return false;
                if (c == '-' && commentBody[i + 1] == '-')
                    return false;
            }
            return true;
        }

        // '<?' PITarget (S (Char* - (Char* '?>' Char*)))? '?>'
        // Name - (('X' | 'x') ('M' | 'm') ('L' | 'l'))
        private bool ReadProcessingInstruction(string toCheckString) =>
            // Check for the '<?' and '?>' at the start and end resprectively
            AssertContainedAndUpdate(toCheckString, 6, "<?", "?>", out string body) &&
            // PITarget
            CheckReference(body, ReadName,
                match => match.StartIndex != 0 || match.Result.ToLower() == "xml",
                match => match.Length == body.Length, out body) &&
            // (S (Char* - (Char* '?>' Char*)))
            CheckReference(body, IsSpace,
                match => match.StartIndex != 0 || match.EndIndex != body.EndIndex,
                match => true, out body) &&
            // (Char* - (Char* '?>' Char*))
            !body.All(c => ValidateCharacter(c.ToString())) && !body.Contains("?>");

        // S 'version' Eq ("'" VersionNum "'" | '"' VersionNum '"')
        private bool ReadVersionInfo(string toCheckString) =>
            ReadDeclaration(toCheckString, "value", 3, ReadVersionNum);

        // S? '=' S?
        private bool ReadEquals(string toCheckString) =>
            ReadOptional(toCheckString, 0, IsSpace, out toCheckString) &&
            ReadOptional(toCheckString, 1, IsSpace, out toCheckString) &&
            toCheckString == "=";

        // '1.' [0-9]+
        private bool ReadVersionNum(string toCheckString) => 
            isXML10 ? toCheckString == "1.0" : toCheckString == "1.1";

        // Comment | PI | S
        private bool ReadMisc(string toCheckString) => 
            ReadComment(toCheckString) || ReadProcessingInstruction(toCheckString) || IsSpace(toCheckString);
        
        private bool ReadCharacterReference(string toCheckString)
        {
            // '&#' [0-9]+ ';' | '&#x' [0-9a-fA-F]+ ';'
            if (!toCheckString.EndsWith(';') &&
                AssertMinLength(toCheckString, 4))
                return false;
            char referenced;

            int semicolonIndex = toCheckString.IndexOf(';');
            string numberAsString = toCheckString.Substring(new Range { StartIndex = 2, EndIndex = semicolonIndex }).Replace("x", "");
            referenced = (char)((toCheckString.StartsWith("&#x") ? Constants.IntFromHex(numberAsString) : int.Parse(numberAsString)));

            return ValidateCharacter(referenced.ToString());
        }

        // EntityRef | CharRef
        private bool ReadReference(string toCheckString) => 
            ReadEntityReference(toCheckString) || ReadCharacterReference(toCheckString);

        // '&' Name ';'
        private bool ReadEntityReference(string toCheckString) => 
            ReadReference(toCheckString, '&');

        // '%' Name ';'
        private bool ReadParsedEntityReference(string toCheckString) => 
            ReadReference(toCheckString, '%');

        // S 'encoding' Eq ('"' EncName '"' | "'" EncName "'" )
        private bool ReadEncodingDecleration(string toCheckString) => 
            ReadDeclaration(toCheckString, "encoding", 1, ReadEncodingName);

        // [A-Za-z] ([A-Za-z0-9._] | '-')*    /* Encoding name contains only Latin characters */
        private bool ReadEncodingName(string toCheckString) => 
            Constants.RegexMatch(toCheckString, "[A-Za-z]([A-Za-z0-9._]|-)*");

        public bool ReadReference(string str, char start) =>
            str.ContainedWithin(start, ';') &&
            ReadName(str.Substring(new Range { StartIndex = 1, EndIndex = str.IndexOf(';') }));

        public bool ReadDeclaration(string toCheckString, string name, int valueCheckLen, Func<string, bool> valueCheck)
        {
            const int spaceLen = 1;
            int nameLen = name.Length;
            const int equalsLen = 1;
            const int quoteLen = 1;
            int expectedLen = spaceLen + nameLen + equalsLen + quoteLen + valueCheckLen + quoteLen; // = nameLen + valueCheckLen + 4
            // S <name> Eq ("'" <valueCheck> "'" | '"' <valueCheck> '"')
            if (!AssertMinLength(toCheckString, expectedLen)) // string has to be at least (nameLen + valueCheckLen + 4) characters long
                return false;
            // S
            if (!CheckReference(toCheckString, IsSpace,                                     // nameLen + valueCheckLen + 3
                match => match.StartIndex != 0 || match.Length >= toCheckString.Length - (expectedLen -= spaceLen),
                match => true, out string leftOver) && !leftOver.StartsWith(name))
                return false;
            leftOver = leftOver.Remove(0, nameLen);
            // Eq
            if (!CheckReference(leftOver, ReadEquals,                                                    // valueCheckLen + 2
                match => match.StartIndex != 0 || match.Length >= toCheckString.Length - (expectedLen -= nameLen + equalsLen),
                match => true, out leftOver))
                return false;
            // ("'" <valueCheck> "'" | '"' <valueCheck> '"')
            if (!AssertQuotedAndUpdate(leftOver, out leftOver))
                return false;
            return valueCheck(leftOver);
        }
    }
}
