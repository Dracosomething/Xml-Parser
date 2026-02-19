using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlParser.src;
using XmlParser.src.xml;

namespace xml_parser.src.EBNF
{
    enum EBNFToken
    {
        HEXADECIMAL,
        RANGE,
        COLLECTION,
        NOT_RANGE,
        NOT_COLLECTION,
        STRING,
        GROUP,
        OR,
        OPTIONAL,
        FOLLOWED,
        NOT,
        PLUS,
        STAR,
        REFERENCE
    }

    internal class EBNFParser
    {
        /*
         * name is a reference to another expression
         * #xn n is an hexadecimal integer represents a character.
         * [a-zA-Z], [#xN-#xN] range, matches any character in that range.
         * [abc], [#xN#xN#xN] collection, matches any of these characters.
         * [^a-z], [^#xN-#xN] not range, matches any character not in the range.
         * [^abc], [^#xN#xN#xN] not collection, matches any character not in the collection.
         * "string" or 'string' matches the literal string
         * (expression) a group of mutliple instructions
         * A? matches a or nothing
         * A B matches a followed by b
         * A | B matches a or b
         * A - B matches a but not b
         * A+ matches one or more occurenses of a
         * A* matches zero or more occurenses of a
         * /* ... * / comment
         * [ wfc: ... ] well-formedness constraint; this identifies by name a constraint on well-formed documents associated with a production.
         * [ vc: ... ] validity constraint; this identifies by name a constraint on valid documents associated with a production.
         */
        private string path;

        public EBNFParser(string path)
        {
            if (!Path.Exists(path))
                throw new FileNotFoundException(path);
            this.path = path;
        }

        public string GetExpression(int index, char? version = null)
        {
            var line = File.ReadLines(path)
                .FirstOrDefault(line => line.StartsWith($"[{index}{(version == null ? "" : version)}]"));
            return line == null ? "" : line;
        }

        public string GetExpression(string name)
        {
            var line = File.ReadLines(path)
                .FirstOrDefault(line => Constants.RegexRemove(line, Constants.index).TrimStart().StartsWith(name));
            return line == null ? "" : line;
        }

        public bool Check(string text, int index)
        {
            var decleration = GetExpression(index);
            var expression = decleration.Split("::=").Last();
            

        }

        private Dictionary<string, EBNFToken> Tokenize(string expression)
        {
            var tokens = new Dictionary<string, EBNFToken>();
            var reader = new FileReader(expression);
            while (!reader.EndOfFile()){
                var token = reader.Read(@"\s");
                EBNFToken type;
                switch(token)
                {
                    case var tokenCase when (tokenCase.StartsWith('"') && tokenCase.EndsWith('"')) || (tokenCase.StartsWith('\'') && tokenCase.EndsWith('\'')):
                        type = EBNFToken.STRING;
                        break;
                    case var tokenCase when tokenCase.StartsWith("#x"):
                        type = EBNFToken.HEXADECIMAL;
                        break;
                    case var tokenCase when tokenCase.StartsWith('(') && tokenCase.EndsWith(')'):
                        type = EBNFToken.GROUP;
                        break;
                    case var tokenCase when tokenCase.EndsWith('+'):
                        type = EBNFToken.PLUS;
                        break;
                    case var tokenCase when tokenCase.EndsWith('?'):
                        type = EBNFToken.OPTIONAL;
                        break;
                    case var tokenCase when tokenCase.EndsWith('*'):
                        type = EBNFToken.STAR;
                        break;
                    default:
                        type = EBNFToken.REFERENCE;
                        break;
                }
                tokens.Add(token, type);
            }
            return tokens;
        }
    }
}
