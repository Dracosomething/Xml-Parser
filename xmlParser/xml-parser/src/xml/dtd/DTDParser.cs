using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlParser.src.xml;

namespace xml_parser.src.xml.dtd
{
    public class DTDFileException : Exception
    {
        public DTDFileException(FileInfo file) : base($"{file.FullName} is not a XML file.") { }
    }

    internal class DTDParser
    {
        private FileInfo DTDFile;
        private StringBuilder content;

        public string Content { get { return content.ToString(); } }

        public DTDParser(string filepath)
        {
            DTDFile = new FileInfo(filepath);
            if (DTDFile.Extension != ".dtd")
                throw new DTDFileException(DTDFile);
            content = new StringBuilder();
            Parse();
        }

        private Exception Parse()
        {
            // ignore: "<!--"
            try
            {
                var reader = new FileReader(DTDFile);
                while (!reader.EndOfFile())
                {
                    string line = readElement(reader);
                }
            }
            catch (Exception e) { return e; }
            return null;
        }

        private string readElement(FileReader reader)
        {
            string line = "";
            for (; ; )
            {
                string c = reader.Read();
                line += c;
                if (c == ">")
                    break;
            }
            return line;
        }
    }
}
