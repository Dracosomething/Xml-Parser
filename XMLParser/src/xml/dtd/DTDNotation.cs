using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlParser.src.xml;

namespace xml_parser.src.xml.dtd
{
    internal class DTDNotation
    {
        private string name;
        private ExternalId externalID;
        private string publicID;
        private string systemID;
        private string systemLiteral;

        public DTDNotation(string name, string publicID, string systemID, ExternalId externalId)
        {
            if (externalId != ExternalId.PUBLIC)
                throw new ArgumentException();
            this.name = name;
            this.publicID = publicID;
            this.systemID = systemID;
            this.externalID = externalId;
        }

        public DTDNotation(string name, string otherString, ExternalId externalId)
        {
            switch (externalID)
            {
                case ExternalId.PUBLIC:
                    this.publicID = otherString;
                    break;
                case ExternalId.SYSTEM:
                    this.systemLiteral = otherString;
                    break;
            }
            this.name = name;
            this.externalID = externalId;
        }
    }
}
