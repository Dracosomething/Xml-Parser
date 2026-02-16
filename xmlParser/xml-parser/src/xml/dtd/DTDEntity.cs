using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xml_parser.src.xml.dtd
{
    internal class DTDEntity
    {
        private string name;
        private string value;

        public DTDEntity(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }
}
