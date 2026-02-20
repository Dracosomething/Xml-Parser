using System;
using System.Collections.Generic;
using System.Text;

namespace XMLParser.src.EBNF
{
    internal class EBNFTokens
    {
        private Dictionary<int, EBNFExpression> tokens = new Dictionary<int, EBNFExpression>();
        private int index = 0;

        public bool Add(EBNFExpression expression) => tokens.TryAdd(index++, expression);

        public bool Get(int index, out EBNFExpression expression)
        {
            return tokens.TryGetValue(index, out expression);
        }
    }
}
