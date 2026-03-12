using System.Text;

namespace XmlParser.src.xml
{
    internal class Tripple<A, B, C>
    {
        public A? First { get; init; }
        public B? Second { get; init; }
        public C? Third { get; init; }

        public override string ToString() =>
            new StringBuilder("[")
            // if Key is null append a string literal "null" otherwise turn Key into a string.
            .Append(First == null ? "null" : First.ToString()).Append(", ")
            .Append(Second == null ? "null" : Second.ToString()).Append(", ")
            .Append(Third == null ? "null" : Third.ToString()).Append("]").ToString();
    }
}
