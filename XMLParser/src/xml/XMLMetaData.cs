using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlParser.src.xml
{
    internal enum ExternalId
    {
        SYSTEM,
        PUBLIC
    }

    internal struct XMLMetaData
    {
        public string version;
        public Encoding encoding;
        public bool standalone;

        public string doctypeName;
        public ExternalId externalID;
        public string fileName;
        public string publicIdentifier;
        public Uri systemIdentifier;

        
    }
}
