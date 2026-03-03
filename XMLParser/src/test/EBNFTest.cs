using System;
using System.Collections.Generic;
using System.Text;
using XmlParser.src.EBNF;
using XmlParser.src.xml;

namespace XmlParser.src.test
{
    internal class EBNFTest
    {
        public static void Run()
        {
            EBNFValidator validator = new EBNFValidator();
            FileReader reader = new FileReader(new FileInfo("./Resources/testData.txt"));
            int linenum = 1;
            while(!reader.EndOfFile())
            {
                string line = reader.ReadLine();
                string[] data = line.Split('`');
                string check = data[0];
                int index = int.Parse(data[1]);
                bool expect = bool.Parse(data[2]);

                validator.ReadyValidatorForUse(index, "./Resources/xml10.ebnf");
                bool res = validator.Validate(check);

                if (res == expect)
                {
                    Console.WriteLine($"{linenum}: {check} is correct");
                } else
                {
                    Console.Write($"{linenum}: {check} vailed, data\n");
                    Console.WriteLine(validator.ToString());
                }
                linenum++;
                validator = new();
            }
        }
    }
}
