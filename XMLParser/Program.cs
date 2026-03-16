using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using XmlParser.src;
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
            // parsing steps
            // step 1: check for dtd
            // step 1.1: if dtd exists do pre processor and replace all references and load in entity declarations.
            // step 1.2: parse dtd and load everything declared into memory
            // step 1.3: get root element
            // step 2: run pre prossesor over xml file
            // step 3: parse xml file
            string input = "((a,  b)|  c| werd|(o,b , ( o| p)) )";
            var matches = Regex.Matches(input, @"(?<=\()\((?>[^()]+|\((?<Depth>)|\)(?<-Depth>))*(?(Depth)(?!))\)");
            foreach(System.Text.RegularExpressions.Match match in matches)
            {
                Console.WriteLine(match.Value);
            }



            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
