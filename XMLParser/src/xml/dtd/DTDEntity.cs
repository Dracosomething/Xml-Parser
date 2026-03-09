using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlParser.src.xml.dtd
{
    internal class DTDEntity
    {
        public string Name { get; init; }
        public string Value { get; init; }
        public bool IsGlobal { get; init; }
        public bool Initialized { get; init; }

        public DTDEntity(string name, string value, bool isGlobal)
        {
            Name = name;
            Value = value;
            IsGlobal = isGlobal;
            Initialized = true;
        }

        public DTDEntity()
        {
            Initialized = false;
        }
    }
}
