using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using XmlParser.src.EBNF;
using XmlParser.src.gui;

namespace XmlParser
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //var parser = new DTDParser("./Resources/dummy.dtd");
            //Console.WriteLine();
            var validator = new EBNFParser("./Resources/xml10.ebnf");
            //string toCheck = "<!-- declare the parameter entity \"ISOLat2\"... -->";
            //var result = validator.Validate(toCheck, 15);
            string another = "<!ELEMENT p (#PCDATA|a|ul|b|i|em)*>";
            var secondResult = validator.Validate(another, 45);
            Console.WriteLine(/*result + */"    " + secondResult);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
