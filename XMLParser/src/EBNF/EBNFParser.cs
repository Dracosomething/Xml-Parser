using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xml_parser.src.EBNF
{
    internal class EBNFParser
    {
        /*
         * #xn - n is an hexadecimal integer represents a character.
         * [a-zA-Z], [#xN-#xN] range, matches any character in that range.
         * [abc], [#xN#xN#xN] collection, matches any of these characters.
         * [^a-z], [^#xN-#xN] not range, matches any character not in the range.
         * [^abc], [^#xN#xN#xN] not collection, matches any character not in the collection.
         * "string" or 'string' matches the literal string
         * (expression) a group
         */
    }
}
