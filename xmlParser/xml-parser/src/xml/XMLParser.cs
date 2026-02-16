using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
        int xmlDecLen = 5;
        int signleCharLen = 1;
        int versionLen = 7;
        char[] quotations = new char[] { '\'', '\"' };

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
                        reader.Skip(signleCharLen);
                        if (reader.Read(versionLen) != "version")
                            throw new Exception("Xml declaration does not start with a version declaration.");
                        reader.Skip(signleCharLen);
                        if (!reader.Read(signleCharLen).SequenceEqual(quotations))
                            throw new Exception("Propertie is not in quotations");

                    }
                }
            } catch(Exception e) { return e; }
            return null;
        }

        private KeyValuePair<string, string> ReadProperty(FileReader reader)
        {

        }
    }
}
