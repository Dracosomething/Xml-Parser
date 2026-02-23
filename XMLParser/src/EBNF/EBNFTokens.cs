using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using xml_parser.src.EBNF;
using xml_parser.src.xml;

namespace XMLParser.src.EBNF
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
    enum EBNFUnificationRule 
    {
        AND,
        NOT,
        OR
    }

    enum EBNFQuantifier
    {
        ONE,
        OPTIONAL,
        MULTIPLE,
        OPT_MULTIPLE
    }

    internal class EBNFTokens
    {
        private Dictionary<int, Tripple<EBNFUnificationRule, EBNFQuantifier, EBNFExpression>> tokens = 
            new Dictionary<int, Tripple<EBNFUnificationRule, EBNFQuantifier, EBNFExpression>>();
        private int index = 0;

        public EBNFExpression this[int i]
        {
            get
            {
                EBNFExpression expression;
                Get(i, out expression);
                return expression;
            }
            set
            {
                if (tokens.ContainsKey(i))
                    tokens.Remove(i);
                tokens.Add(i, new Tripple<EBNFUnificationRule, EBNFQuantifier, EBNFExpression>
                {
                    First = EBNFUnificationRule.AND,
                    Second = EBNFQuantifier.ONE,
                    Third = value
                });
            }
        }

        public bool this[int i, EBNFUnificationRule rule]
        {
            get
            {
                return SetRule(rule, i);
            }
        }

        public bool this[int i, EBNFQuantifier quantifier]
        {
            get
            {
                return SetQuantifier(quantifier, i);
            }
        }

        public bool SetQuantifier(EBNFQuantifier quantifier, int index)
        {
            var tripple = new Tripple<EBNFUnificationRule, EBNFQuantifier, EBNFExpression>();
            var resultOne = tokens.TryGetValue(index, out tripple);
            if (!resultOne)
                return false;
            var updated = new Tripple<EBNFUnificationRule, EBNFQuantifier, EBNFExpression>
            {
                First = tripple.First,
                Second = quantifier,
                Third = tripple.Third
            };
            tokens.Remove(index);
            return tokens.TryAdd(index, updated);
        }

        public bool SetRule(EBNFUnificationRule rule, int index)
        {
            var tripple = new Tripple<EBNFUnificationRule, EBNFQuantifier, EBNFExpression>();
            var resultOne = tokens.TryGetValue(index, out tripple);
            if (!resultOne)
                return false;
            var updated = new Tripple<EBNFUnificationRule, EBNFQuantifier, EBNFExpression>
            {
                First = rule,
                Second = tripple.Second,
                Third = tripple.Third
            };
            tokens.Remove(index);
            return tokens.TryAdd(index, updated);
        }

        public bool Add(EBNFExpression expression) => 
            tokens.TryAdd(index++, new Tripple<EBNFUnificationRule, EBNFQuantifier, EBNFExpression>
            {
                First = EBNFUnificationRule.AND,
                Second = EBNFQuantifier.ONE,
                Third = expression
            });

        public bool Get(int index, out EBNFExpression expression)
        {
            var tripple = new Tripple<EBNFUnificationRule, EBNFQuantifier, EBNFExpression>();
            var res = tokens.TryGetValue(index, out tripple);
            expression = tripple.Third;
            return res;
        }

        public bool Validate(string check)
        {
            int index = 0;
            int oldIndex = index;
            bool setCurrent = true;
            bool current = false;
            List<bool> groups = new List<bool>();
            int groupIndex = 0;
            for(int i = 0; i < this.index; i++)
            {
                if (index >= check.Length)
                    break;
                bool toAdd;
                char c = check[index++];
                var token = tokens[i];
                var rule = token.First;
                var quantifier = token.Second;
                var expression = token.Third;
                bool expressionResult = Check(c, check, quantifier, expression, ref index);

                if (rule == EBNFUnificationRule.NOT)
                {
                    index = oldIndex;
                    var nextToken = tokens[++i];
                    var nextQuantifier = nextToken.Second;
                    var nextExpression = nextToken.Third;
                    bool next = !Check(c, check, nextQuantifier, nextExpression, ref index);
                    // a - b
                    // input is abefe
                    // output is false 
                    toAdd = next ? false : expressionResult;
                }
                else
                    toAdd = expressionResult;
                
                if (setCurrent)
                    current = toAdd;
                else
                    current = current && toAdd;
                setCurrent = false;
                
                if (rule == EBNFUnificationRule.OR)
                {
                    groups[groupIndex++] = current;
                    current = false;
                    setCurrent = true;
                }
            }
            return groups.All(b => b);
        }

        private bool Check(char c, string check, EBNFQuantifier quantifier, EBNFExpression expression, ref int index)
        {
            bool expressionResult = false;
            switch (quantifier)
            {
                case EBNFQuantifier.ONE:
                    if (expression.Rule == EBNFToken.STRING)
                        expressionResult = expression.Check(check, ref index);
                    expressionResult = expression.Check(c);
                    break;
                case EBNFQuantifier.OPTIONAL:
                    if (expression.Rule == EBNFToken.STRING)
                        expression.Check(check, ref index);
                    expression.Check(c);
                    break;
                case EBNFQuantifier.MULTIPLE:
                    if (expression.Rule == EBNFToken.STRING)
                        expressionResult = expression.Check(check, ref index);
                    expressionResult = expression.Check(c);
                    while (expression.Check(check[index++])) ;
                    break;
                case EBNFQuantifier.OPT_MULTIPLE:
                    index--;
                    while (expression.Check(check[index++])) ;
                    break;
            }
            return expressionResult;
        }
    }
}
