using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlParser.src.xml
{
    internal class FileReader
    {
        private string text;
        private int index = 0;

        public FileReader(FileInfo info)
        {
            text = File.ReadAllText(info.FullName);
        }

        public string Read(int amount = 1)
        {
            if (index + amount >= this.text.Length)
            {
                return null;
            }
            string read = this.text.Substring(index, amount);
            index += amount;
            return read;
        }

        public string Peak(int amount = 1)
        {
            if (index + amount >= this.text.Length)
            {
                return null;
            }
            return this.text.Substring(index, amount);
        }

        public bool Skip(int amount = 1)
        {
            if (index + amount >= this.text.Length)
            {
                return false;
            }
            index += amount;
            return true;
        }

        public bool Back(int amount = 1)
        {
            if (index - amount < 0)
            {
                return false;
            }
            index -= amount;
            return true;
        }

        public bool EndOfFile()
        {
            return this.index >= this.text.Length - 1;
        }
    }
}
