using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlParser.src.xml
{
    internal class Tripple<A, B, C>
    {
        public A First { get; init; }

        public B Second { get; init; }
        public C Third { get; init; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");

            // if Key is null append a string literal "null" otherwise turn Key into a string.
            builder.Append(First == null ? "null" : First.ToString());
            builder.Append(",");
            builder.Append(Second == null ? "null" : Second.ToString());
            builder.Append(",");
            builder.Append(Third == null ? "null" : Third.ToString());

            builder.Append("]");
            return base.ToString();
        }
    }
}
