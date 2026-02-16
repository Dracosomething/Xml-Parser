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
        NOTATION
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
        private string name;
        private DTDDataType dataType;
        private DTDDeclarationType declarationType;
        private string data;

        public DTDAttList(string elementName, string name, DTDDataType dataType, DTDDeclarationType declarationType, string data)
        {
            this.elementName = elementName;
            this.name = name;
            this.dataType = dataType;
            this.declarationType = declarationType;
            this.data = data;
        }
    }
}
