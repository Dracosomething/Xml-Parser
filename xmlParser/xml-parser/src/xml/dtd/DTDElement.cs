using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xml_parser.src.xml.dtd
{
    internal class DTDElement
    {
        private string name;
        private DTDSequence sequence;

        public DTDElement(string name, DTDSequence sequence)
        {
            this.name = name;
            this.sequence = sequence;
        }
    }
}
