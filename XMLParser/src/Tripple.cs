using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xml_parser.src.xml
{
    internal class Tripple<A, B, C>
    {
        private A first;
        private B second;
        private C third;

        public A First
        {
            get { return first; }
        }

        public B Second
        {
            get { return second; }
        }
        public C Third
        {
            get { return third; }
        }

        public Tripple(A first, B second, C third )
        {
            this.first = first;
            this.second = second;
            this.third = third;
        }

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
