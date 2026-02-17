using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xml_parser.src.xml.dtd
{
    internal class DTDAttribute
    {
        private string name;
        private Pair<DTDDataType, List<string>> datatype;
        private Pair<DTDDeclarationType, string> attributeDefault;

        public string Name
        {
            get => name;
        }

        public Pair<DTDDataType, List<string>> DataType
        {
            get => datatype;
        }

        public Pair<DTDDeclarationType, string> AttributeDefault
        {
            get => attributeDefault;
        }

        public DTDAttribute(string name, DTDDataType dataType, DTDDeclarationType declarationType, string value = null)
        {
            if (declarationType == DTDDeclarationType.FIXED && value == null)
                throw new ArgumentNullException("value");
            if (dataType == DTDDataType.ENUMERATION || dataType == DTDDataType.NOTATION)
                throw new ArgumentException("Param can not be of an enumeration type", "dataType");
            this.name = name;
            this.datatype = new Pair<DTDDataType, List<string>>(dataType, null);
            this.attributeDefault = new Pair<DTDDeclarationType, string>(declarationType, value);
        }

        public DTDAttribute(string name, DTDDataType dataType, List<string> enumeration, DTDDeclarationType declarationType, string value = null)
        {
            if (declarationType == DTDDeclarationType.FIXED && value == null)
                throw new ArgumentNullException("value");
            if (dataType != DTDDataType.ENUMERATION || dataType != DTDDataType.NOTATION)
                throw new ArgumentException("Param must be of an enumeration type", "dataType");
            this.name = name;
            this.datatype = new Pair<DTDDataType, List<string>>(dataType, enumeration);
            this.attributeDefault = new Pair<DTDDeclarationType, string>(declarationType, value);
        }
    }
}
