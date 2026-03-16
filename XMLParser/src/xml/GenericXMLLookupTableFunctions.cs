using XmlParser.src.extentions;

namespace XmlParser.src.xml
{
    internal partial class GenericXMLLookupTable : BaseLookupTableFunctions
    {
        // #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
        private bool ValidateCharacterXML10(string toCheckString) =>
            (Utils.RegexMatch(toCheckString, "\\t|\\n|\\r|[\\s-\\uD7FF]|[\\uE000-\\uFFFD]") ||
            (toCheckString[0] >= 0x10000 && toCheckString[0] <= 0x10FFFF));

        // 	[#x1-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
        private bool ValidateCharacterXML11(string toCheckString) =>
            (Utils.RegexMatch(toCheckString, "[\\x01-\\uD7FF]|[\\uE000-\\uFFFD]") || (toCheckString[0] >= 0x10000 && toCheckString[0] <= 0x10FFFF));

        private bool ValidateCharacter(string toCheckString) =>
            AssertCharacter(toCheckString) &&
            isXML10 ? ValidateCharacterXML10(toCheckString) : ValidateCharacterXML11(toCheckString);

        // (#x20 | #x9 | #xD | #xA)+
        private bool IsSpace(string toCheckString) =>
            AssertMinLength(toCheckString, 1) &&
            Utils.RegexMatch(toCheckString, "(\\s|\\t|\\r|\\n)+");

        // [#x1-#x8] | [#xB-#xC] | [#xE-#x1F] | [#x7F-#x84] | [#x86-#x9F]
        private bool IsRestrictedCharacter(string toCheckString) =>
            AssertCharacter(toCheckString) &&
            Utils.RegexMatch(toCheckString, "[\\x01-\\x08]|[\\x0B-\\x0C]|[\\x0E-\\x1F]|[\\x7F-\\x84]|[\\x86-\\x9F]");

        // ":" | [A-Z] | "_" | [a-z] | [#xC0-#xD6] | [#xD8-#xF6] | [#xF8-#x2FF] | [#x370-#x37D] | [#x37F-#x1FFF] | [#x200C-#x200D] |
        // [#x2070-#x218F] | [#x2C00-#x2FEF] | [#x3001-#xD7FF] | [#xF900-#xFDCF] | [#xFDF0-#xFFFD] | [#x10000-#xEFFFF]
        private bool ReadNameStartCharacter(string toCheckString) =>
            AssertCharacter(toCheckString) &&
            (Utils.RegexMatch(toCheckString, ":|[A-Z]|_|[a-z]|[À-Ö]|[Ø-ö]|[ø-˿]|[Ͱ-ͽ]|[Ϳ-\\u1FFF]|[\\u200C-\\u200D]|[⁰-\\u218F]|[Ⰰ-\\u2fef]|" +
                "[、-\\uD7FF]|[豈-\\uFDCF]|[\\uFDF0-\\uFFFD]") || (toCheckString[0] >= 0x10000 && toCheckString[0] <= 0xEFFFF));

        //  NameStartChar | "-" | "." | [0-9] | #xB7 | [#x0300-#x036F] | [#x203F-#x2040]
        private bool ReadNameCharacter(string toCheckString) =>
            AssertCharacter(toCheckString) &&
            (ReadNameStartCharacter(toCheckString) || // unicode 203f to unicode 2040
             Utils.RegexMatch(toCheckString, "-|\\.|[0-9]|·|[\\u0300-\\u036F]|[‿-⁀]"));

        // 	NameStartChar (NameChar)*
        private bool ReadName(string toCheckString) =>
            AssertMinLength(toCheckString, 1) && (
                toCheckString.Length > 1 ? (
                    toCheckString.StartsWith(ReadNameStartCharacter) &&
                    toCheckString.Substring(1).AllString(ReadNameCharacter)
                ) : toCheckString.StartsWith(ReadNameStartCharacter)
            );

        // Name (#x20 Name)*
        private bool ReadNames(string toCheckString) => ReadMultipleTokens(toCheckString, ' ', ReadName);

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
                match => match.StartIndex != 0 && match.Result.ToLower() == "xml",
                match => match.Length == body.Length,
                out body) &&
            // (S (Char* - (Char* '?>' Char*)))
            CheckReference(body, IsSpace,
                match => match.StartIndex != 0 && match.EndIndex != body.EndIndex,
                match => true,
                out body) &&
            // (Char* - (Char* '?>' Char*))
            !body.AllString(ValidateCharacter) && !body.Contains("?>");

        // S 'version' Eq ("'" VersionNum "'" | '"' VersionNum '"')
        private bool ReadVersionInfo(string toCheckString) =>
            ReadDeclaration(toCheckString, "version", 3, ReadVersionNum);

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
            referenced = (char)((toCheckString.StartsWith("&#x")) ? Utils.IntFromHex(numberAsString) : int.Parse(numberAsString));

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
            Utils.RegexMatch(toCheckString, "[A-Za-z]([A-Za-z0-9._]|-)*");

        public bool ReadReference(string str, char start) =>
            AssertContainedAndUpdate(str, 3, start.ToString(), ";", out string name) &&
            ReadName(name);

        public bool ReadDeclaration(string toCheckString, string name, int valueCheckLen, Func<string, bool> valueCheck)
        {
            // S <name> Eq ("'" <valueCheck> "'" | '"' <valueCheck> '"')
            const int spaceLen = 1;
            int nameLen = name.Length;
            const int equalsLen = 1;
            const int quoteLen = 1;
            int expectedLen = spaceLen + nameLen + equalsLen + quoteLen + valueCheckLen + quoteLen; // = nameLen + valueCheckLen + 4
            if (!AssertMinLength(toCheckString, expectedLen) || // string has to be at least (nameLen + valueCheckLen + 4) characters long
                                                                // S
                (!CheckReference(toCheckString, IsSpace,                                     // nameLen + valueCheckLen + 3
                match => match.StartIndex != 0 && match.Length >= toCheckString.Length - (expectedLen -= spaceLen),
                match => true,
                out string leftOver) &&
                !leftOver.StartsWith(name)))
                return false;
            if (leftOver == null)
                return false;
            leftOver = leftOver.Remove(0, nameLen);
            // Eq
            if (!CheckReference(leftOver, ReadEquals,                                                    // valueCheckLen + 2
                match => match.StartIndex != 0 && match.Length >= toCheckString.Length - (expectedLen -= nameLen + equalsLen),
                match => true,
                out leftOver))
                return false;
            // ("'" <valueCheck> "'" | '"' <valueCheck> '"')
            if (!AssertQuotedAndUpdate(leftOver, out leftOver))
                return false;
            return valueCheck(leftOver);
        }
    }
}
