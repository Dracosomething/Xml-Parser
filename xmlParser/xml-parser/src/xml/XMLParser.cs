using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using xml_parser.src.xml;

namespace XmlParser.src.xml
{
    public class XMLFileException : Exception
    {
        public XMLFileException(FileInfo file) : base($"{file.FullName} is not a XML file.") { }
    }

    internal class XMLParser
    {
        private FileInfo XMLFile;
        private StringBuilder content;
        private int xmlDecLen = 5;
        private int singleCharLen = 1;
        private int versionLen = 7;
        private int equalsLen = 3;
        private IList<string> quotations = new List<string> { "'", "\"" };
        

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
            // ignore: "<!--"
            try
            {
                var reader = new FileReader(XMLFile);
                int flag = 0;
                
                while (!reader.EndOfFile())
                {
                    if (flag == 0)
                    {
                        if (reader.Peak(xmlDecLen) != "<?xml")
                            continue;
                        flag = 1;
                        reader.Skip(xmlDecLen);
                        reader.Skip(singleCharLen);
                        if (reader.Read(versionLen) != "version")
                            throw new Exception("Xml declaration does not start with a version declaration.");
                        reader.Skip(singleCharLen);
                        if (!quotations.Contains(reader.Read(singleCharLen)))
                            throw new Exception("Propertie is not in quotations");

                    }
                }
            } catch(Exception e) { return e; }
            return null;
        }

        private Pair<string, string> ReadProperty(FileReader reader)
        {
            Pair<string, string> result;
            

            string key = "";
            string current;
            string tmp;
            while (true)
            {
                tmp = reader.Peak(3);
                
                current = reader.Read();
                key += current;
            }



            if (reader.Peak() != "\"" || reader.Peak() != "'")
                throw new Exception("Xml property seems to be malformed.");
            reader.Skip();
            string value = "";
            // we do an infinite loop here to make it easier when breaking out of it
            while(true)
            {
                current = reader.Read();
                if (quotations.Contains(current))
                    break;
                value += current;
            }

            result = new Pair<string, string>(key, value);
            return result;
        }
    }
}
