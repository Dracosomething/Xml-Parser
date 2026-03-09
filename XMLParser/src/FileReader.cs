using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XmlParser.src;

namespace XmlParser.src
{
    internal class FileReader : IDisposable
    {
        private string text;
        private int index = 0;

        public FileReader(FileInfo info)
        {
            this.text = File.ReadAllText(info.FullName);
        }

        public FileReader(string fileContents)
        {
            this.text = fileContents;
        }

        public string Read(int amount = 1)
        {
            if (index + amount >= this.text.Length)
                return null;
            string read = this.text.Substring(index, amount);
            index += amount;
            return read;
        }

        public string ReadLine()
        {
            string res = "";
            while (true)
            {
                string char_ = this.Read();
                if (char_ == "\n")
                    return res;
                res += char_;
            } 
        }

        public string Read(Func<string, bool> func)
        {
            var match = ReadFirstMatch(func);
            if (match.StartIndex == 0)
                return Read(match.Length);
            else
                return Read(match.StartIndex);
        }

        public string Peak(int amount = 1)
        {
            if (index + amount >= this.text.Length)
                return null;
            return this.text.Substring(index, amount);
        }

        public bool Skip(int amount = 1)
        {
            if (index + amount >= this.text.Length)
                return false;
            index += amount;
            return true;
        }

        public bool Skip(Func<string, bool> func)
        {
            var match = ReadFirstMatch(func);
            if (match.StartIndex == 0)
                return Skip(match.Length);
            else
                return Skip(match.StartIndex);
        }

        public bool Back(int amount = 1)
        {
            if (index - amount < 0)
                return false;
            index -= amount;
            return true;
        }

        public bool EndOfFile() => this.index >= this.text.Length - 1;

        private Match ReadFirstMatch(Func<string, bool> func)
        {
            // We only need to check the part of the file data that hasn't been read yet.
            string toCheck = text.Substring(index);
            return toCheck.FirstMatch(func);
        }

        public void Dispose()
        {
            this.text = null;
            this.index = 0;
        }
    }
}
