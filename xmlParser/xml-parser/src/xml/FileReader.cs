using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XmlParser.src.xml
{
    internal class FileReader : IDisposable
    {
        private string text;
        private int index = 0;

        public FileReader(FileInfo info)
        {
            text = File.ReadAllText(info.FullName);
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

        public string Read(string regex)
        {
            var reg = new Regex(regex);
            Match match = ReadFirstMatch(reg);
            return Read(match.Length);
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

        public bool SkipRegex(string regex)
        {
            var reg = new Regex(regex);
            Match match = ReadFirstMatch(reg);
            int toSkip = match.Length;
            return Skip(toSkip);
        }

        public bool Back(int amount = 1)
        {
            if (index - amount < 0)
                return false;
            index -= amount;
            return true;
        }

        public bool EndOfFile()
        {
            return this.index >= this.text.Length - 1;
        }

        private Match ReadFirstMatch(Regex regex)
        {
            // We only need to check the part of the file data that hasn't been read yet.
            string toCheck = text.Substring(index);
            MatchCollection matches = regex.Matches(toCheck);
            if (matches.Count == 0)
                return null;
            Match match = matches[0];
            if (!match.Success)
                return null;
            return match;
        }

        public void Dispose()
        {
            this.text = null;
            this.index = 0;
        }
    }
}
