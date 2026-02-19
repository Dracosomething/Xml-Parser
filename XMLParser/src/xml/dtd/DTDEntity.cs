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
        private bool global;

        public string Name { get => name; }
        public string Value { get => value; }
        public bool IsGlobal { get => global; }

        public DTDEntity(string name, string value, bool global)
        {
            this.name = name;
            this.value = value;
            this.global = global;
        }
    }
}
