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
            Match match = ReadFirstMatch(regex);
            string leftOver = this.text.Substring(index);
            if (leftOver.StartsWith(match.Value))
                return Read(match.Length);
            else
            {
                int indexFound = match.Index;
                int toReadAmount = this.index - indexFound;
                return Read(toReadAmount);
            }
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
            Match match = ReadFirstMatch(regex);
            string leftOver = this.text.Substring(index);
            if (leftOver.StartsWith(match.Value))
                return Skip(match.Length);
            else
            {
                int indexFound = match.Index;
                int toReadAmount = this.index - indexFound;
                return Skip(toReadAmount);
            }
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

        private Match ReadFirstMatch(string regex)
        {
            // We only need to check the part of the file data that hasn't been read yet.
            string toCheck = text.Substring(index);
            Console.WriteLine();
            MatchCollection matches = Regex.Matches(toCheck, regex);
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
