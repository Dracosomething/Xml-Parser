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
    internal class GenericXMLLookupTable
    {
        private LookupTable<string, Func<string, bool>> table = new(
             new Dictionary<string, Func<string, bool>>()
             {
                { "Char_XML10"      , ValidateCharacterXML10            },
                { "Char_XML11"      , ValidateCharacterXML11            },
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
                { "EncName"         , ReadEncodingName                  }
             }
        );

        public Func<string, bool> this[string str] => table[str];

        private static bool ValidateCharacterXML10(string toCheckString)
        {
            // 	#x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
            if (toCheckString.Length != 1)
                return false;
            char toCheckChar = toCheckString[0];
            // basicaly check if toCheckChar is not the surrogate block unicode characters, hexadecimal 0xFFFE, and hexadecimal 0xFFFF. 
            return Constants.RegexMatch(toCheckString, "\\t|\\n|\\r|[\\s-\\uD7FF]|[\\uE000-\\uFFFD]") || (toCheckChar >= 0x10000 && toCheckChar <= 0x10FFFF);
        }

        private static bool ValidateCharacterXML11(string toCheckString)
        {
            // 	[#x1-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
            if (toCheckString.Length != 1)
                return false;
            char toCheckChar = toCheckString[0];
            // basicaly check if toCheckChar is not the surrogate block unicode characters, hexadecimal 0xFFFE, and hexadecimal 0xFFFF. 
            return Constants.RegexMatch(toCheckString, "[\\x01-\\uD7FF]|[\\uE000-\\uFFFD]") || (toCheckChar >= 0x10000 && toCheckChar <= 0x10FFFF);
        }

        // (#x20 | #x9 | #xD | #xA)+
        private static bool IsSpace(string toCheckString) => Constants.RegexMatch(toCheckString, "(\\s|\\t|\\r|\\n)+");

        private static bool IsRestrictedCharacter(string toCheckString)
        {
            // [#x1-#x8] | [#xB-#xC] | [#xE-#x1F] | [#x7F-#x84] | [#x86-#x9F]
            if (toCheckString.Length != 1)
                return false;
            return Constants.RegexMatch(toCheckString, "[\\x01-\\x08]|[\\x0B-\\x0C]|[\\x0E-\\x1F]|[\\x7F-\\x84]|[\\x86-\\x9F]");
        }

        private static bool ReadNameStartCharacter(string toCheckString)
        {
            // ":" | [A-Z] | "_" | [a-z] | [#xC0-#xD6] | [#xD8-#xF6] | [#xF8-#x2FF] | [#x370-#x37D] | [#x37F-#x1FFF] | [#x200C-#x200D] |
            // [#x2070-#x218F] | [#x2C00-#x2FEF] | [#x3001-#xD7FF] | [#xF900-#xFDCF] | [#xFDF0-#xFFFD] | [#x10000-#xEFFFF]
            if (toCheckString.Length != 1)
                return false;
            char toCheckChar = toCheckString[0];
            return Constants.RegexMatch(toCheckString, ":|[A-Z]|_|[a-z]|[À-Ö]|[Ø-ö]|[ø-˿]|[Ͱ-ͽ]|[Ϳ-\\u1FFF]|[\\u200C-\\u200D]|[⁰-\\u218F]|[Ⰰ-\\u2fef]|" +
                "[、-\\uD7FF]|[豈-\\uFDCF]|[\\uFDF0-\\uFFFD]") || (toCheckChar >= 0x10000 && toCheckChar <= 0xEFFFF);
        }

        private static bool ReadNameCharacter(string toCheckString)
        {
            //  NameStartChar | "-" | "." | [0-9] | #xB7 | [#x0300-#x036F] | [#x203F-#x2040]
            if (toCheckString.Length != 1)
                return false;
            return ReadNameStartChar(toCheckString) ||          // unicode 203f to unicode 2040
                Constants.RegexMatch(toCheckString, "-|\\.|[0-9]|·|[\\u0300-\\u036F]|[‿-⁀]");
        }

        private static bool ReadName(string toCheckString)
        {
            // 	NameStartChar (NameChar)*
            if (toCheckString.Length < 1)
                return false;
            string firstCharacter = toCheckString.First().ToString();
            // nameStartChar
            bool result = ReadNameStartChar(firstCharacter);
            // only check further if we have more string left
            if (toCheckString.Length > 1)
            {
                string leftOverToCheck = toCheckString.Substring(1);
                // (NameChar)*
                foreach (char c in leftOverToCheck)
                {
                    // NameChar
                    if (!ReadNameChar(c.ToString()))
                        break; // break here since this would be the last occurance of c
                }
            }
            // in the end only the result matters
            return result;
        }

        private static bool ReadNames(string toCheckString)
        {
            // Name (#x20 Name)*
            if (toCheckString.Length < 1)
                return false;
            string[] items = toCheckString.Split(' '); // split all the names;
            if (items.Length < 1)
                return false;
            bool result = ReadName(items[0]); // check if the first one is a valid name and store it sepperately
            foreach (var item in items)
            {
                if (!ReadName(item))
                    break; // when the rest no longer matches the name syntax break the loop since all our names have been found.
            }
            return result; // only result matters
        }

        private static bool ReadAttributeValue(string toCheckString)
        {
            // '"' ([^<&"] | Reference)* '"' | "'" ([^<&'] | Reference)* "'"
            if (toCheckString.Length < 2) // make shure string is at least 2 characters long
                return false;
            bool quoted = toCheckString.ContainedWithin('"') || toCheckString.ContainedWithin("'");
            if (!quoted) // if the string is not within quotes we can savely return false
                return false;
            char quote = toCheckString.First();
            string unquoted = toCheckString.Remove(0, 1).Remove(toCheckString.Length-1, 1);
            return Constants.RegexMatch(unquoted, $"[^<{quote}]*"); // we can ignore references since they will already be included this way.
        }

        private static bool ReadSystemLiteral(string toCheckString)
        {
            // ('"' [^"]* '"') | ("'" [^']* "'")
            if (toCheckString.Length < 2) // make shure string is at least 2 characters long
                return false;
            bool quoted = toCheckString.ContainedWithin('"') || toCheckString.ContainedWithin("'");
            if (!quoted) // if the string is not within quotes we can savely return false
                return false;
            char quote = toCheckString.First();
            string unquoted = toCheckString.Remove(0, 1).Remove(toCheckString.Length - 1, 1);
            return Constants.RegexMatch(unquoted, $"[^{quote}]*");
        }
    
        private static bool ReadPubidLiteral(string toCheckString)
        {
            // '"' PubidChar* '"' | "'" (PubidChar - "'")* "'"
            if (toCheckString.Length < 2) // make shure string is at least 2 characters long
                return false;
            bool quoted = toCheckString.ContainedWithin('"') || toCheckString.ContainedWithin("'");
            if (!quoted) // if the string is not within quotes we can savely return false
                return false;
            char quote = toCheckString.First();
            string unquoted = toCheckString.Remove(0, 1).Remove(toCheckString.Length - 1, 1);
            // only if quote is '"' should we check if the string has '\''
            return Constants.RegexMatch(unquoted, $"\\s|\\r|\\n|[a-zA-Z0-9]|[-{(quote == '"' ? '\'' : "")}()+,./:=?;!*#@$_%]");
        }

        private static bool ReadComment(string toCheckString)
        {
            // '<!--' ((Char - '-') | ('-' (Char - '-')))* '-->'
            if (toCheckString.Length < 7) // string has to be at least 7 characters long
                return false;
            bool result = toCheckString.StartsWith("<!--") && toCheckString.EndsWith("-->");
            if (!result)
                return false;
            var commentBody = toCheckString.Replace("<!--", "").Replace("-->", "");
            for (int i = 0; i < commentBody.Length; i++)
            {
                var c = commentBody[i];
                if (c == '-')
                    if (commentBody[i + 1] == '-')
                        return false;
                if (!ValidateCharacterXML10(c.ToString()))
                    return false;
            }
            return result;
        }

        private static bool ReadProcessingInstruction(string toCheckString)
        {
            // '<?' PITarget (S (Char* - (Char* '?>' Char*)))? '?>'
            // Name - (('X' | 'x') ('M' | 'm') ('L' | 'l'))
            if (toCheckString.Length < 6) // string has to be at least 5 characters long
                return false;
            bool contained = toCheckString.StartsWith("<?") && toCheckString.EndsWith("?>"); // starting '<?' and ending '?>' check
            if (!contained)
                return false;
            string body = toCheckString.Remove(0, 2).Replace("?>", ""); // remove the <? and ?>
            var match = body.FirstMatch(ReadName); // PITarget
            if (!match.Found || match.StartIndex != 0 || match.Result.ToLower() == "xml") // if the match was not found, if it wasn't at the start, or it is any form of xml
                return false;
            if (match.Length == body.Length) // check for if we encountered the end of the body
                return true;
            body = body.Remove(match.StartIndex, match.Length);
            // (S (Char* - (Char* '?>' Char*)))
            match = body.FirstMatch(IsSpace); // S
            if (!match.Found || match.StartIndex != 0 || match.EndIndex != body.Length - 1)
                return false;
            body = body.Remove(match.StartIndex, match.Length);
            // (Char* - (Char* '?>' Char*))
            if (!body.All(c => ValidateCharacterXML10(c.ToString())))
                return false;
            return !body.Contains("?>");
        }
    }
}
