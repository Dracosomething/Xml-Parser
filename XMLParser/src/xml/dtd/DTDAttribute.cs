using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlParser.src;

namespace XmlParser.src.xml.dtd
{
    internal class DTDAttribute
    {
        public string Name { get; }
        public Pair<DTDDataType, List<string>> DataType { get; }
        public Pair<DTDDeclarationType, string> AttributeDefault { get; }

        public DTDAttribute(string name, DTDDataType dataType, DTDDeclarationType declarationType, string value = null)
        {
            if (declarationType == DTDDeclarationType.FIXED && value == null)
                throw new ArgumentNullException("value");
            if (dataType == DTDDataType.ENUMERATION || dataType == DTDDataType.NOTATION)
                throw new ArgumentException("Param can not be of an enumeration type", "dataType");
            Name = name;
            Datatype = new Pair<DTDDataType, List<string>>
            {
                Key = dataType,
                Value = null
            };
            AttributeDefault = new Pair<DTDDeclarationType, string>
            {
                Key = declarationType,
                Value = value
            };
        }

        public DTDAttribute(string name, DTDDataType dataType, List<string> enumeration, DTDDeclarationType declarationType, string value = null)
        {
            if (declarationType == DTDDeclarationType.FIXED && value == null)
                throw new ArgumentNullException("value");
            if (dataType != DTDDataType.ENUMERATION || dataType != DTDDataType.NOTATION)
                throw new ArgumentException("Param must be of an enumeration type", "dataType");
            Name = name;
            Datatype = new Pair<DTDDataType, List<string>>
            {
                Key = dataType,
                Value = enumeration
            };
            AttributeDefault = new Pair<DTDDeclarationType, string>
            {
                Key = declarationType,
                Value = value
            };
        }
    }
}
