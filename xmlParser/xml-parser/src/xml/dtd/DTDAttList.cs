using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xml_parser.src.xml.dtd
{
    enum DTDDataType
    {
        CDATA,
        ID,
        IDREF,
        IDREFS,
        ENTITY,
        ENTITIES,
        NMTOKEN,
        NMTOKENS,
        NOTATION,
        ENUMERATION
    }

    enum DTDDeclarationType
    {
        REQUIRED,
        IMPLIED,
        FIXED
    }

    internal class DTDAttList
    {
        private string elementName;
        private List<DTDAttribute> attributes;

        public DTDAttList(string elementName)
        {
            this.elementName = elementName;
            this.attributes = new List<DTDAttribute>();
        }

        public void Add(DTDAttribute item) 
        {
            attributes.Add(item);
        }

        public DTDAttribute this[int i]
        {
            get
            {
                return this.attributes[i]; 
            }
            set
            {
                this.attributes[i] = value;
            }
        }
    }
}
