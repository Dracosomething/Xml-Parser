using System;
using System.Collections.Generic;
using System.Text;
using XmlParser.src.EBNF;
using XmlParser.src.xml;

namespace XmlParser.src.EBNF
{
    internal class EBNFExpression
    {
        public EBNFToken Rule { get; init; }
        public string Token { get; init; }
        public Type Type { get; init; }
        public object Data { get; init; }

        /// <summary>
        /// rule HEXADECIMAL stores data as an integer<br/>
        /// rule COLLECTION stores data as an EBNFCollection<br/>
        /// rule STRING and rule POINTER stores data as a string<br/>
        /// rule GROUP and rule REFERENCE store data as EBNFTokens
        /// </summary>
        /// <typeparam name="T">The type of the output data.</typeparam>
        /// <param name="output">The data to output</param>
        /// <returns>true if data was succesfully extracted.</returns>
        public bool GetData<T>(out T output)
        {
            if (typeof(T) == Type && Data is T)
            {
                output = (T)Data;
                return true;
            }
            output = default!;
            return false;
        }

        public bool Equals(EBNFExpression other)
        {
            return other.Rule == Rule && other.Token == Token && other.Data == Data && other.Type == Type;
        }

        public string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("rule: ").Append(Rule).AppendLine();
            builder.Append("token: ").Append(Token).AppendLine();
            builder.Append("type: ").Append(Type.ToString()).AppendLine();
            builder.Append("data: ").Append(Data).AppendLine();
            return builder.ToString();
        }
    }
}