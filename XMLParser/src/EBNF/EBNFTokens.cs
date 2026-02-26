using XmlParser.src.xml;

namespace XmlParser.src.EBNF
{
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

        public int Size { 
            get
            {
                return tokens.Count;
            } 
        }

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

        public bool GetToken(int index, out Tripple<EBNFUnificationRule, EBNFQuantifier, EBNFExpression>? token)
        {
            var res = tokens.TryGetValue(index, out Tripple<EBNFUnificationRule, EBNFQuantifier, EBNFExpression>? tripple);
            token = tripple;
            if (tripple == null)
                return false;
            return res;
        }
    }
}
