using System.Collections;
using System.Linq.Expressions;
using XmlParser.src;
using XmlParser.src.EBNF;
using XmlParser.src.xml;

/*  ==============How the stream for an EBNF expression should look like==============
 *  
 *  input string
 *  while string has char left
 *      get current character
 *      if quotation
 *          read characters untill closeing quotation
 *          if character is \\
 *              check if next character is the closing notation
 *              add to result
 *          end if
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
 *      else
 *          read untill next space
 *          get referenced expression
 *          parse referenced expression
 *      end if
 *  end while
 */

namespace XmlParser.src.EBNF
{
    enum EBNFToken
    {
        HEXADECIMAL,
        COLLECTION,
        STRING,
        GROUP,
        REFERENCE,
        POINTER
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
        private Dictionary<string, EBNFTokens> parsed = new Dictionary<string, EBNFTokens>();

        public Dictionary<string, EBNFTokens> Parsed
        {
            get => parsed;
        }

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
                .FirstOrDefault(line =>
                {
                    var tmp = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (tmp.Length == 0)
                        return false;
                    return tmp[1] == name;
                });
            return line == null ? "" : line;
        }

        public EBNFTokens Ready(int index)
        {
            var decleration = GetExpression(index);
            var tokenized = decleration.Split("::=");
            var name = tokenized.First().Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
            EBNFTokens tokens;
            if (parsed.ContainsKey(name))
                tokens = parsed[name];
            else
            {
                var expression = decleration.Split("::=").Last().TrimStart(' ');
                bool result = false;
                int indexText = 0;
                List<string> tmp = new List<string>();
                tokens = Parse(expression, ref tmp);
                parsed[name] = tokens;
            }
            return tokens;
        }

        private EBNFTokens Parse(string expresion, ref List<string> stack)
        {
            var expressions = new EBNFTokens();
            int expressionsIndex = -1;
            for (int i = 0; i < expresion.Length; i++)
            {
                char c = expresion[i];
                if (c == ' ')
                    continue;
                switch (c)
                {
                    case var _ when c == '"' || c == '\'':
                        expressions.Add(ReadString(expresion, c, ref i));
                        expressionsIndex++;
                        break;
                    case var _ when c == '(':
                        expressions.Add(ReadGroup(expresion, ref stack, ref i));
                        expressionsIndex++;
                        break;
                    case var _ when c == '[':
                        expressions.Add(ReadCollection(expresion, ref i));
                        expressionsIndex++;
                        break;
                    case var _ when c == '#' && expresion[i + 1] == 'x':
                        i++;
                        expressions.Add(ReadHexadecimal(expresion, ref i));
                        expressionsIndex++;
                        break;
                    case var _ when c == '|':
                        expressions.SetRule(EBNFUnificationRule.OR, expressionsIndex);
                        break;
                    case var _ when c == '-':
                        expressions.SetRule(EBNFUnificationRule.NOT, expressionsIndex);
                        break;
                    case var _ when c == '+':
                        expressions.SetQuantifier(EBNFQuantifier.MULTIPLE, expressionsIndex);
                        break;
                    case var _ when c == '*':
                        expressions.SetQuantifier(EBNFQuantifier.OPT_MULTIPLE, expressionsIndex);
                        break;
                    case var _ when c == '?':
                        expressions.SetQuantifier(EBNFQuantifier.OPTIONAL, expressionsIndex);
                        break;
                    default:
                        expressions.Add(ReadReference(expresion, ref stack, ref i));
                        expressionsIndex++;
                        break;
                }
            }
            return expressions;
        }

        private EBNFExpression ReadReference(string expression, ref List<string> stack, ref int index)
        {
            string result = "";
            char c;
            while ((c = expression[index++]) != ' ' && c != '*' && c != '?' && c != '+')
            {
                result += c;
                if (index == expression.Length)
                {
                    index++;
                    break;
                }
            }
            // go back one since we still need the next characters
            index -= 2;
            if(stack.Contains(result))
            {
                return new EBNFExpression
                {
                    Rule = EBNFToken.POINTER,
                    Token = result,
                    Type = typeof(string),
                    Data = result
                };
            }
            EBNFTokens tokens;
            // check if we have already parsed this expression before so that we dont parse anything twice.
            if (parsed.ContainsKey(result))
                tokens = parsed[result];
            else
            {
                var decleration = GetExpression(result);
                var expressionFound = decleration.Split("::=").Last().TrimStart(' ');
                stack.Add(result);
                tokens = Parse(expressionFound, ref stack);
                stack.Remove(result);
                parsed[result] = tokens;
            }
            return new EBNFExpression
            {
                Rule = EBNFToken.REFERENCE,
                Token = result,
                Type = typeof(EBNFTokens),
                Data = tokens
            };
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
            while (true)
            {
                if (expression[index] == ']')
                    break;
                // skip the first 2 characters since they are allways going to be #x
                index += 2;
                int num;
                var hexadecimal = ReadHexadecimal(expression, ref index);
                result += $"#x{hexadecimal.Token}";
                var success = hexadecimal.GetData(out num);
                if (!success)
                    break;
                char next = expression[index];
                if (next == '-')
                {
                    index += 2;
                    result += '-';
                    int num2;
                    hexadecimal = ReadHexadecimal(expression, ref index);
                    result += $"#x{hexadecimal.Token}";
                    success = hexadecimal.GetData(out num2);
                    if (!success)
                        break;
                    var range = new Range
                    {
                        Start = num,
                        End = num2
                    };
                    collection.Add(range);
                    continue;
                }
                collection.Add(num);
                if (index + 1 >= expression.Length)
                    break;
            }
        }

        private void ReadCollectionNormal(string expression, ref int index, ref EBNFCollection collection, ref string result)
        {
            char c;
            while (index + 1 < expression.Length && (c = expression[++index]) != ']')
            {
                char next = expression[index + 1];
                if (c == '\\')
                {
                    if (next == ']')
                    {
                        result += next;
                        index++;
                        continue;
                    }
                }
                result += c;
                if (next == '-')
                {
                    char rangeEnd = expression[index + 2];
                    index += 2;
                    result += $"-{rangeEnd}";
                    var range = new Range
                    {
                        Start = c,
                        End = rangeEnd
                    };
                    collection.Add(range);
                    continue;
                }
                collection.Add(c);
            }
        }

        private EBNFExpression ReadHexadecimal(string expression, ref int index)
        {
            string result = "";
            char c;
            int length = 0;
            while (index + 1 < expression.Length && (c = expression[++index]) != ' ' && c != '-' && c != '#' && c != ']')
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

        private EBNFExpression ReadGroup(string expression, ref List<string> stack, ref int index)
        {
            string result = "";
            char c;
            int nestingCount = 1;
            while (true) 
            {
                if (index + 1 >= expression.Length)
                    break;
                c = expression[++index];
                if (c == '(')
                    nestingCount++;
                if (c == ')')
                    nestingCount--;
                if (nestingCount == 0 && c == ')')
                    break;
                result += c;
            }
            ;
            EBNFTokens tokens = Parse(result, ref stack);
            return new EBNFExpression
            {
                Rule = EBNFToken.GROUP,
                Token = '(' + result + ')',
                Type = typeof(EBNFTokens),
                Data = tokens
            };
        }

        private EBNFExpression ReadString(string expression, char end, ref int index)
        {
            string result = "";
            char c;
            while (index + 1 < expression.Length && (c = expression[++index]) != end) 
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
