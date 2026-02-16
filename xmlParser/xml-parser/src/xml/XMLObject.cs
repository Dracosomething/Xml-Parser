using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlParser.src.xml
{
    internal class XMLObject
    {
        private string name;
        private Dictionary<string, string> properties;
        private Dictionary<string, List<XMLObject>> children;
    }
}
