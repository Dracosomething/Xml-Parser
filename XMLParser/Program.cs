using XmlParser.src.gui;
using XmlParser.src.xml;

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

            // special attribute: xml:space
            // when declared must be an enumeration type that can only hold the values of "default" or "preserve"
            // anything else throws an error
            // "default" means the parsers default whitespace handling behaviour aka ignore whitespace
            // "preserve" means preseerving whitespace
            // value applies to all nested xml elements
            // default value should be specified by the element declaration

            // prosessor must normalize all instances of "#xD #xA" and any "#xD" not followed by "#xA" to a single "#xA"

            // special attribute: xml:lang
            // must be declared if it's used
            // value must be a language identifier as defined in https://datatracker.ietf.org/doc/html/rfc4646 and https://datatracker.ietf.org/doc/html/rfc4647
            // type must be character data

            // implement namespaces later on
            // defined in https://www.w3.org/TR/xml-names/

            var now = DateTime.Now;
            string fname = $"./logs/{now.Hour}-{now.Minute}-{now.Second}.{now.Millisecond}_{now.Day}-{now.Month}-{now.Year}_log.txt";
            using (TextWriter writer = new StreamWriter(fname))
            {
                DateTime start;
                DateTime end;
                writer.WriteLine($"start: {start = DateTime.Now}");
                string targetFile = "./Resources/dummyStandalone.xml";
                var parser = new XMLParser(targetFile);
                var processed = parser.Parse();
                writer.WriteLine($"end: {end = DateTime.Now}");
                var duration = end.Subtract(start);
                // add pretty print option 
                writer.WriteLine($"took {duration} to compute {processed}");
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
