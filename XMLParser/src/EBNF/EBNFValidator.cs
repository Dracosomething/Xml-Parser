using System;
using System.Collections.Generic;
using System.Text;

namespace XmlParser.src.EBNF
{
    internal class EBNFValidator
    {
        /*
         * will handle all the needed things for validation
         * needs token and referenced items
         * 
         * needs to keep track of which element is currently being checked
         * 
         */
        private EBNFTokens tokens;
        private Dictionary<int, string> referenced;
        private EBNFExpression current;

        public EBNFValidator(EBNFTokens tokens, Dictionary<int, string> referenced)
        {
            this.tokens = tokens;
            this.referenced = referenced;
        }
    }
}
