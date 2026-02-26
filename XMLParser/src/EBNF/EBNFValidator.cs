using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using XmlParser.src.xml;
using XmlParser.src.EBNF;

namespace XmlParser.src.EBNF
{
    /*  ======================Validate function pseudo code======================
     *  input string
     *  result boolean
     *  while tokens left
     *      if string has no characters left
     *          end loop
     *      end if
     *      get char from string at index
     *      get token from tokens
     *      get rule from token
     *      get quantifier from token
     *      get expression from token
     *      
     *      if quantifier is one
     *          check expression
     *      end if
     *      if quantifier is optional
     *          update index if expression is present
     *      end if
     *      if quantifier is mutliple
     *          check expression
     *          if expression not present
     *              not valid
     *          end if
     *          while char matches expression
     *              move index forward one
     *          end while
     *      end if
     *      if quantifier is optional multiple
     *          while char matches expression
     *              move index forward one
     *          end while
     *      end if
     *      
     *      if rule is and
     *          add if valid to result linked by and
     *      end if
     *      if rule is not
     *          check next token
     *          check if current token is correct but not next token
     *      end if
     *      if rule is or
     *          get tokens untill next or
     *          check if any are correct
     *      end if
     *  end while
     */
    internal class EBNFValidator
    {
        /*
         * will handle all the needed things for validation
         * needs token and referenced items
         * 
         * needs to keep track of which element is currently being checked
         * 
         */
        // our current tokens
        private EBNFTokens tokens;
        // a dictionary that contains all our expression to reference
        private Dictionary<string, EBNFTokens> referenced;
        private EBNFExpression currentExpression;
        private int tokensIndex;
        private int checkIndex;
        // dictionary where the key is an integer which is the index and the value is a pair where
        // int is the index of the current handled expression in the provided EBNFTokens
        private Dictionary<int, Pair<int, EBNFTokens>> expressionStack = new Dictionary<int, Pair<int, EBNFTokens>>();
        private int expressionStackIndex = 0;
        // saved pointer data, key is a range with the start index of checking and the index of the start of the next token, the value is pointerdata
        // which is a wrapper over two integers, an EBNFQuantifier and the name of the pointer
        private Dictionary<Range, PointerData> saved = new Dictionary<Range, PointerData>();
        private string checking;
        private List<List<bool>> groups = new List<List<bool>>();
        private int indexOfGroup = 0;
        private int indexOfBoolInGroup = 0;

        public EBNFValidator()
        {
            
        }

        private EBNFValidator(EBNFTokens tokens, Dictionary<string, EBNFTokens> referenced, Dictionary<int, Pair<int, EBNFTokens>> stack, int stackIndex)
        {
            this.tokens = tokens;
            this.referenced = referenced;
            this.expressionStack = stack;
            this.expressionStackIndex = stackIndex;
        }

        /// <summary>
        /// prepares validator to be able to be used
        /// </summary>
        /// <param name="expressionIndex"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool ReadyValidatorForUse(int expressionIndex, string filePath)
        {
            var parser = new EBNFParser(filePath);
            this.tokens = parser.Ready(expressionIndex);
            this.referenced = parser.Parsed;
            return this.tokens != null;
        }

        public bool Validate(string toCheck)
        {
            this.groups.Add(new List<bool>());
            checking = toCheck;
            checkIndex = 0;
            int oldCheckIndex = checkIndex;
            for (tokensIndex = 0; tokensIndex < tokens.Size; tokensIndex++)
            {
                if (checkIndex >= toCheck.Length)
                    return false;
                bool toAdd;
                var success = tokens.GetToken(tokensIndex, out var token);
                if (!success)
                    return false;
                var rule = token.First;
                var quantifier = token.Second;
                this.currentExpression = token.Third;

                toAdd = Check(quantifier);
                if (rule == EBNFUnificationRule.NOT)
                {
                    checkIndex = oldCheckIndex;
                    success = tokens.GetToken(++tokensIndex, out var next);
                    if (!success)
                        return false;
                    var nextResult = CheckSpecificExpression(next.Third, next.Second);
                    toAdd = nextResult ? false : toAdd;
                }

                var group = this.groups[indexOfGroup];
                group.Add(toAdd);
                this.groups[indexOfGroup] = group;

                if (rule == EBNFUnificationRule.OR)
                {
                    checkIndex = oldCheckIndex;
                    this.indexOfGroup++;
                    this.groups.Add(new List<bool>());
                    this.indexOfBoolInGroup = -1; // we make it negative one since we will increment it right after we set it.
                }
                this.indexOfBoolInGroup++;
                oldCheckIndex = checkIndex;
            }
            var result = groups.Any(group => group.All(boolean => boolean));
            if (result)
            {
                foreach(var pointer in saved)
                {
                    var tokenData = pointer.Value;
                    var positionData = pointer.Key;
                    var subString = toCheck.Substring(positionData.Start, positionData.Start - positionData.End);
                    checking = subString;
                    var tokenName = tokenData.PointerName;
                    var token = referenced[tokenName];

                    var quantifier = tokenData.Quantifier;
                    var group = groups[tokenData.GroupIndex];

                    var newValidator = new EBNFValidator(tokens, referenced, expressionStack, expressionStackIndex);
                    var newCheck = newValidator.Validate(checking);

                    group[tokenData.IndexInGroup] = newCheck;
                    groups[tokenData.GroupIndex] = group;
                }
                result = groups.Any(group => group.All(boolean => boolean));
            }
            return result;
        }

        private bool CheckSpecificExpression(EBNFExpression token, EBNFQuantifier quantifier)
        {
            var current = this.currentExpression;
            this.currentExpression = token;
            var result = Check(quantifier);
            this.currentExpression = current;
            return result;
        }

        private int NextTokenIndex()
        {
            var last = expressionStack.Last().Value ;
            var success = last.Value.GetToken(last.Key + 1, out var nextToken);
            if (!success || nextToken == null)
                return -1;
            int nextIndex = tokensIndex;

            while (!CheckSpecificExpression(nextToken.Third, nextToken.Second))
            {
                nextIndex++;
            }

            return nextIndex;
        }

        private bool CheckCurrentExpression(EBNFQuantifier quantifier)
        {
            if (checkIndex >= checking.Length)
                return false;
            bool success;
            char c;
            switch(currentExpression.Rule)
            {
                case EBNFToken.POINTER:
                    success = this.currentExpression.GetData(out string name);
                    if (!success)
                        return false;

                    int start = this.checkIndex;
                    int nextTokenIndex = NextTokenIndex();

                    var lengthRange = new Range
                    {
                        Start = start,
                        End = nextTokenIndex
                    };

                    var pointerData = new PointerData
                    {
                        IndexInGroup = this.indexOfBoolInGroup,
                        GroupIndex = this.indexOfGroup,
                        Quantifier = quantifier,
                        PointerName = name
                    };

                    saved.Add(lengthRange, pointerData);
                    return true;
                case EBNFToken.REFERENCE or EBNFToken.GROUP:
                    success = this.currentExpression.GetData(out EBNFTokens tokens);
                    if (!success)
                        return false;
                    
                    if (currentExpression.Rule == EBNFToken.REFERENCE)
                        this.expressionStack.Add(expressionStackIndex++, new Pair<int, EBNFTokens> { Key = tokensIndex, Value = this.tokens });

                    var validator = new EBNFValidator(tokens, this.referenced, this.expressionStack, expressionStackIndex);
                    string toCheck = this.checking.Substring(this.checkIndex);
                    var result = validator.Validate(toCheck);
                    if (result)
                        this.checkIndex++;

                    if (currentExpression.Rule == EBNFToken.REFERENCE)
                        this.expressionStack.Remove(--expressionStackIndex);
                    return result;
                case EBNFToken.COLLECTION:
                    success = this.currentExpression.GetData(out EBNFCollection collection);
                    if (!success)
                        return false;

                    c = checking[checkIndex++];
                    return collection.Check(c);
                case EBNFToken.HEXADECIMAL:
                    success = this.currentExpression.GetData(out int number);
                    if (!success)
                        return false;

                    c = checking[checkIndex++];
                    return number == c;
                case EBNFToken.STRING:
                    success = this.currentExpression.GetData(out string str);
                    if (!success)
                        return false;

                    string str2 = this.checking.Substring(this.checkIndex, str.Length);
                    this.checkIndex += str.Length;
                    return str2 == str;
            }
            return false;
        }

        private bool Check(EBNFQuantifier quantifier)
        {
            if (currentExpression.Rule == EBNFToken.POINTER)
                return CheckCurrentExpression(quantifier);
            switch(quantifier)
            {
                case EBNFQuantifier.ONE:
                    return CheckCurrentExpression(quantifier);
                case EBNFQuantifier.OPTIONAL:
                    CheckCurrentExpression(quantifier);
                    return true;
                case EBNFQuantifier.MULTIPLE:
                    var result = CheckCurrentExpression(quantifier);
                    while (CheckCurrentExpression(quantifier)) ;
                    return result;
                case EBNFQuantifier.OPT_MULTIPLE:
                    while (CheckCurrentExpression(quantifier)) ;
                    return true;

            }
            return false;
        }
    }
}
