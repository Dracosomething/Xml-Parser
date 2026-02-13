using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace xml_parser.src.xml
{
    public class XMLFileException : Exception
    {
        public XMLFileException(FileInfo file) : base($"{file.FullName} is not a XML file.") { }
    }

    internal class XMLParser
    {
        private FileInfo XMLFile;
        private StringBuilder content;

        public string Content { get { return content.ToString(); } }

        public XMLParser(string filepath)
        {
            XMLFile = new FileInfo(filepath);
            if (XMLFile.Extension != ".xml")
                throw new XMLFileException(XMLFile);
            content = new StringBuilder();
            Parse();
        }

        private Exception Parse()
        {
            try {
                var reader = new FileReader(XMLFile);
                int flag = 0;
                while (!reader.eof())
                {
                    if (flag == 0)
                    {
                        if (reader.Peak(5) == "<?xml")
                        {
                            flag = 1;

                        }
                    }

                }
            } catch(Exception e) { return e; }
            return null;
        }
    }
}
