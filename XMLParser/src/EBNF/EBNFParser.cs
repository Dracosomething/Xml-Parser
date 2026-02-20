using System.Collections;
using XmlParser.src;
using XMLParser.src.EBNF;

/*  ==============How the stream for an EBNF expression should look like==============
 *  
 *  input string
 *  while string has char left
 *      get current character
 *      if quotation
 *          read characters untill closeing quotation
 *      end if
 *      if opening brackets
 *          create sepperate data block
 *          parse expression into data block
 *      end if
 *      if square bracket
 *          read untill closing bracket
 *          if first character is ^
 *              make it not
 *          end if
 *          if character is -
 *              if first character
 *                  stop if
 *              end if
 *              read next character
 *              create range between previous character and next character
 *          end if
 *          add character to collection
 *      end if
 *      if character is |
 *          combine previous expression and next expression into new group with rule of or
 *      end if
 *      if character is -
 *          combine previous expression and next expression into new group with rule of not
 *      end if
 *      if character is +
 *          make current expression have rule of one or more
 *      end if
 *      if character is *
 *          make current expression have rule of zero or more
 *      end if
 *      if character is ?
 *          make current expression have rule of optional
 *      end if
 *      if character is # and next character is x
 *          read untill next #, space or -
 *          remove #x from start
 *          parse rest of string from hexadecimal to decimal
 *      end if
 *      if character is space
 *          start new expression
 *      end if
 *  end while
 */

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
            //var tokens = Tokenize(expression);
            bool result = false;
            int indexText = 0;
            
        }

        private EBNFTokens Parse(string expresion)
        {
            EBNFTokens expressions = new EBNFTokens();
            for (int i = 0; i < expresion.Length; i++)
            {
                char c = expresion[i];
                switch (c)
                {
                    case var _ when c == '"' || c == '\'':
                        expressions.Add(ReadString(expresion, c, ref i));
                        break;
                    case var _ when c == '(':
                        expressions.Add(ReadGroup(expresion, ref i));
                        break;
                    case var _ when c == '[':
                        expressions.Add(ReadCollection(expresion, ref i));
                        break;
                    case var _ when c == '#' && expresion[i + 1] == 'x':
                        expressions.Add(ReadHexadecimal(expresion, ref i));
                        break;

                }

            }
        }

        private EBNFExpression ReadCollection(string expression, ref int index)
        {
            EBNFCollection collection = new EBNFCollection();
            string result = "";
            if (expression[index + 1] == '^')
            {
                collection.Not = true;
                index++;
            }
            char c;
            bool isFirst = true;
            bool isLast = false;
            bool isHex = false;
            if (expression[index + 1] == '#' && expression[index + 2] == 'x')
            {
                isHex = true;
            }
            if (isHex)
                ReadCollectionHex(expression, ref index, ref collection, ref result);
            else
                ReadCollectionNormal(expression, ref index, ref collection, ref result);
            
            return new EBNFExpression
            {
                Rule = EBNFToken.COLLECTION,
                Token = $"[{(collection.Not ? "^" : "")}" + result + "]",
                Type = typeof(EBNFCollection),
                Data = collection
            };
        }

        private void ReadCollectionHex(string expression, ref int index, ref EBNFCollection collection, ref string result)
        {
            bool isLast = false;
            while (true)
            {
                if (isLast)
                    break;
                if (expression[index + 1] == ']')
                    isLast = true;
                int num;
                ReadHexadecimal(expression, ref index).GetData(out num);
                char next = expression[index + 1];
                result += num;
                if (next == '-')
                {
                    index++;
                    result += '-';
                    int num2;
                    ReadHexadecimal(expression, ref index).GetData(out num2);
                    result += num2;
                    var range = new XMLParser.src.EBNF.Range
                    {
                        Start = num,
                        End = num2
                    };
                    collection.Add(range);
                    continue;
                }
                collection.Add(num);
            }
        }

        private void ReadCollectionNormal(string expression, ref int index, ref EBNFCollection collection, ref string result)
        {
            bool isFirst = true;
            bool isLast = false;
            char c;
            while ((c = expression[++index]) != ']')
            {
                if (expression[index + 1] == ']')
                    isLast = true;
                if (c == '\\')
                {
                    if (isFirst)
                        isFirst = false;
                    char next = expression[++index];
                    if (next == ']')
                    {
                        result += next;
                        continue;
                    }
                    else
                        index--;
                }
                result += c;
                if (!isFirst && !isLast && c == '-')
                {
                    char next = expression[++index];
                    char previous = expression[index - 2];
                    var range = new XMLParser.src.EBNF.Range
                    {
                        Start = next,
                        End = previous
                    };
                    collection.Add(range);
                    if (isFirst)
                        isFirst = false;
                    continue;
                }
                collection.Add(c);
            }
        }

        private EBNFExpression ReadHexadecimal(string expression, ref int index)
        {
            string result = "";
            char c;
            index++;
            int length = 0;
            while ((c = expression[++index]) != ' ' || c != '-' || c != '#')
            {
                result += c;
                length++;
            }
            int number = Constants.IntFromHex(result);
            return new EBNFExpression
            {
                Rule = EBNFToken.HEXADECIMAL,
                Token = "#x" + result,
                Type = typeof(int),
                Data = number
            };
        }

        private EBNFExpression ReadGroup(string expression, ref int index)
        {
            string result = "";
            char c;
            int nestingCount = 0;
            while (true) 
            {
                c = expression[++index];
                result += c;
                if (c == '(')
                    nestingCount++;
                if (nestingCount == 0 && c == ')')
                    break;
            };

            return new EBNFExpression
            {
                Rule = EBNFToken.GROUP,
                Token = '(' + result,
                Type = typeof(EBNFTokens),
                Data = Parse(result)
            };
        }

        private EBNFExpression ReadString(string expression, char end, ref int index)
        {
            string result = "";
            char c;
            while ((c = expression[++index]) != end) 
            { 
                if (c == '\\')
                {
                    char next = expression[++index];
                    if (next == end)
                    {
                        result += next;
                        continue;
                    }                    
                    else
                        --index;
                }
                result += c;
            }
            return new EBNFExpression
            {
                Rule = EBNFToken.STRING,
                Token = end + result + end,
                Type = typeof(string),
                Data = result
            };
        }
    }
}
