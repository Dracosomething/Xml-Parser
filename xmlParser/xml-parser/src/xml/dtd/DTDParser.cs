using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XmlParser.src;
using XmlParser.src.xml;

namespace xml_parser.src.xml.dtd
{
    public class DTDFileException : Exception
    {
        public DTDFileException(string file) : base($"{file} is not a XML file.") { }
    }

    internal class DTDParser
    {
        private string filepath;
        private string DTDContents;
        private bool isInternal = false;
        private DTDSchema schema;

        public DTDSchema Schema{ get { return schema; } }

        public DTDParser(string filepath)
        {
            this.filepath = filepath;
            DTDContents = File.ReadAllText(filepath);
            schema = new DTDSchema();
            Parse();
        }

        public DTDParser(Uri external)
        {
            this.DTDContents = Task.Run(() => Constants.httpClient.GetStringAsync(external)).Result;
            schema = new DTDSchema();
            Parse();
        }

        public DTDParser(FileReader fileReader)
        {
            DTDContents = "";
            while(true)
            {
                string line = fileReader.ReadLine();
                if (Constants.RegexMatch(line, Constants.element) || Constants.RegexMatch(line, Constants.sTag))
                    break;
                DTDContents += line;
            }
            this.schema = new DTDSchema();
            this.isInternal = true;
            Parse();
        }

        public Exception Parse()
        {
            // ignore: "<!--"
            try
            {
                var reader = new FileReader(DTDContents);
                reader = new FileReader(ParseParameterEntities(reader));

                // second itteration parse the other items.
                while (!reader.EndOfFile())
                {
                    string line = ReadElement(reader);
                    if (Constants.RegexMatch(line, Constants.comment))
                        continue;
                    switch (line)
                    {
                        case var o when Constants.RegexMatch(o, Constants.conditionalSect) && !isInternal:
                            ParseConditional(line);
                            break;
                        case var o when Constants.RegexMatch(o, Constants.elementdecl):
                            break;
                        case var o when Constants.RegexMatch(o, Constants.attlistDecl):
                            schema.addAtrributeList(ParseAttList(line));
                            break;
                        case var o when Constants.RegexMatch(o, Constants.notationDecl):
                            break;
                        case var o when Constants.RegexMatch(o, Constants.entityDecl):
                            schema.addEntity(ParseEntity(line));
                            break;
                    }
                }

                
            }
            catch (Exception e) { return e; }
            return null;
        }
        
        private string ParseParameterEntities(FileReader reader)
        {
            string processedFileContents = "";
            while (!reader.EndOfFile())
            {
                string line = ReadParameterEntity(reader);
                if (Constants.RegexMatch(line, Constants.comment))
                    continue;
                switch (line)
                {
                    case var o when Constants.RegexMatch(line, Constants.peDecl):
                        this.schema.addEntity(this.ParseEntity(line));
                        break;
                    case var o when Constants.RegexMatch(line, Constants.peReference):
                        line = line.Replace("%", "").Replace(";", "");
                        string result = schema.getEntity(line, true).Value;
                        var readerNested = new FileReader(result);
                        ParseParameterEntities(readerNested);
                        readerNested.Dispose();
                        break;
                    default:
                        processedFileContents += line;
                        break;
                }
            }
            return processedFileContents;
        }

        private string ReadElement(FileReader reader)
        {
            return reader.Read($"({Constants.markupdecl}){(!isInternal ? $"|({Constants.conditionalSect})" : "")}");
        }

        private string ReadParameterEntity(FileReader reader)
        {
            return reader.Read($@"({Constants.peReference})|({Constants.peDecl})");
        }

        private string ReplaceCharReferences(string text, MatchCollection matches)
        {
            var handled = new List<string>();
            foreach (Match match in matches)
            {
                if (handled.Contains(match.Value))
                    continue;
                if (match.Success)
                {
                    char c;
                    string val = match.Value;
                    string number = val.Replace("&#", "").Replace(";", "");
                    if (val.Contains('x'))
                    {
                        number = number.Replace("x", "");
                        c = (char)int.Parse(number, NumberStyles.HexNumber);
                    }
                    else
                        c = (char)int.Parse(number);
                    text = text.Replace(val, $"{c}");
                    handled.Add(val);
                }
            }
            return text;
        }

        private string ReplaceEntityReferences(string text, MatchCollection matches)
        {
            var handled = new List<string>();
            foreach (Match match in matches)
            {
                if (handled.Contains(match.Value))
                    continue;
                if (match.Success)
                {
                    string code = match.Value;
                    string name = code.Replace("&", "").Replace(";", "");
                    DTDEntity entity = this.schema.getEntity(name, false);
                    string value = entity.Value;
                    text = text.Replace(code, value);
                    handled.Add(code);
                }
            }
            return text;
        }

        private string ReplacePEReferences(string text, MatchCollection matches)
        {
            var handled = new List<string>();
            foreach (Match match in matches)
            {
                if (handled.Contains(match.Value))
                    continue;
                if (match.Success)
                {
                    string code = match.Value;
                    string name = code.Replace("%", "").Replace(";", "");
                    DTDEntity entity = this.schema.getEntity(name, true);
                    string value = entity.Value;
                    text = text.Replace(code, value);
                    handled.Add(code);
                }
            }
            return text;
        }

        private string ReplaceReferences(string text)
        {
            var regex = new Regex(Constants.charRef);
            MatchCollection matches = regex.Matches(text);
            text = ReplaceCharReferences(text, matches);
            
            regex = new Regex(Constants.entityRef);
            matches = regex.Matches(text);
            text = ReplaceEntityReferences(text, matches);

            if (!isInternal) 
            {
                regex = new Regex(Constants.peReference);
                matches = regex.Matches(text);
                text = ReplacePEReferences(text, matches);
            }

            return text;
        }

        private void ParseConditional(string item)
        {
            List<char> spaceCharacters = new List<char> { ' ', '\t', '\r', '\n' };
            const string openingBrackets = "<![";
            var reader = new FileReader(item);
            reader.Skip(openingBrackets.Length);
            if (spaceCharacters.Contains(reader.Peak().First()))
                reader.SkipRegex(Constants.space);
            if (Constants.RegexMatch(item, Constants.ignoreSect))
                return;
            const string include = "INCLUDE";
            const string closingBrackets = "]]>";
            reader.Skip(include.Length);
            string toParse = reader.Read(closingBrackets);
            var parser = new DTDParser(new FileReader(toParse));
            schema.Combine(parser.Schema);
        }

        private DTDEntity ParseEntity(string item)
        {
            char[] spaceCharacters = new char[] { ' ', '\t', '\r', '\n' };
            bool global = Constants.RegexMatch(item, Constants.peDecl);
            const string entityHeader = "<!ENTITY";
            var reader = new FileReader(item);
            reader.Skip(entityHeader.Length);
            reader.SkipRegex(Constants.space);
            if (!global){
                // skip '%' character
                reader.Skip();
                reader.SkipRegex(Constants.space);
            }
            string name = reader.Read(Constants.name);
            reader.SkipRegex(Constants.space);
            string value;
            if (global)
                value = reader.Read(Constants.entityDef);
            else
                value = reader.Read(Constants.peDef);
            if (Constants.RegexMatch(value, Constants.externalId))
            {
                switch (value)
                {
                    case var o when o.StartsWith("SYSTEM"):
                        o = o.Remove(0, "SYSTEM".Length);
                        o = o.TrimStart(spaceCharacters);
                        if (Constants.RegexMatch(o, Constants.url))
                            o = Constants.RegexReplace(o, "", Constants.domain);
                        string path = Constants.RegexExtract(filepath, Constants.filePath);
                        var parser = new DTDParser(path + o);
                        schema.Combine(parser.Schema);
                        parser = null;
                        break;
                    case var o when o.StartsWith("PUBLIC"):
                        o = o.Remove(0, "PUBLIC".Length);
                        o = o.TrimStart(spaceCharacters);
                        string metaData = Constants.RegexExtract(o, Constants.pubidLiteral);
                        o = Constants.RegexReplace(o, "", Constants.pubidLiteral);
                        o = o.TrimStart(spaceCharacters);
                        var uri = new Uri(o);
                        var parserPublic = new DTDParser(uri);
                        schema.Combine(parserPublic.Schema);
                        parserPublic = null;
                        break;
                }
            }
            value = ReplaceReferences(value);

            reader.Dispose();
            return new DTDEntity(name, value, global);
        }

        private DTDAttList ParseAttList(string item)
        {
            // define constants
            const string attListHeader = "<!ATTLIST";
            var reader = new FileReader(item);
            int attributeCount = Constants.RegexCount(item, Constants.attDef);

            // return value
            DTDAttList attList;
            // skip header
            reader.Skip(attListHeader.Length);
            // skip space regex
            reader.SkipRegex(Constants.space);
            // read name
            string elementName = reader.Read(Constants.name);
            attList = new DTDAttList(elementName);
            // read attdef
            for (int i = 0; i < attributeCount; i++)
            {
                DTDAttribute newElement = ParseAttribute(reader);
                attList.Add(newElement);
            }
            reader.Dispose();
            return attList;
        }

        private DTDAttribute ParseAttribute(FileReader reader)
        {
            // constants
            char[] spaceCharacters = new char[] { ' ', '\t', '\r', '\n' };
            string fixedRegex = $@"((#FIXED{Constants.space})?{Constants.attValue})";
            
            // eventueal result
            DTDAttribute newElement;
            // extract the attribute definition
            string attributeDef = reader.Read(Constants.attDef);
            // create extra reader
            var attDefReader = new FileReader(attributeDef);

            attDefReader.SkipRegex(Constants.space);
            // read element name
            string name = attDefReader.Read(Constants.name);
            attDefReader.SkipRegex(Constants.space);

            // read attribute type
            string attributeType = attDefReader.Read(Constants.attType);
            DTDDataType dataType;
            List<string> sequence = null;
            // parse attribute type
            switch (attributeType)
            {
                case var o when Constants.RegexMatch(o, Constants.notationType):
                    dataType = DTDDataType.NOTATION;
                    int index = attributeType.IndexOf('(');
                    string sequenceString = attributeType.Substring(index);
                    sequence = ParseSequence(sequenceString, Constants.name);
                    break;
                case var o when Constants.RegexMatch(o, Constants.enumeration):
                    dataType = DTDDataType.ENUMERATION;
                    sequence = ParseSequence(attributeType, Constants.nmToken);
                    break;
                default:
                    dataType = (DTDDataType)Enum.Parse(typeof(DTDDataType), attributeType);
                    break;
            }
            attDefReader.SkipRegex(Constants.space);
            // read default declaration
            string defaultDeclaration = attDefReader.Read(Constants.defaultDecl);
            DTDDeclarationType declarationType;
            string defaultValue = null;
            // parse default declaration
            switch (defaultDeclaration)
            {
                case var o when Constants.RegexMatch(o, fixedRegex):
                    declarationType = DTDDeclarationType.FIXED;
                    defaultValue = defaultDeclaration.Replace("#FIXED", "").TrimStart(spaceCharacters);
                    defaultValue = ReplaceReferences(defaultValue);
                    break;
                case var o when Constants.RegexMatch(o, Constants.attValue):
                    declarationType = DTDDeclarationType.FIXED;
                    defaultValue = defaultDeclaration;
                    defaultValue = ReplaceReferences(defaultValue);
                    break;
                default:
                    declarationType = (DTDDeclarationType)Enum.Parse(typeof(DTDDeclarationType), defaultDeclaration);
                    break;
            }
            if (sequence == null)
                newElement = new DTDAttribute(name, dataType, declarationType, defaultValue);
            else
                newElement = new DTDAttribute(name, dataType, sequence, declarationType, defaultValue);
            attDefReader.Dispose();
            return newElement;
        }

        private List<string> ParseSequence(string sequence, string regex)
        {
            var sequenceObj = new List<string>();
            var reader = new FileReader(sequence);
            string[] elements = sequence.Split('|');
            reader.Skip();
            foreach (string member in elements)
            {
                if (Constants.RegexMatch(reader.Peak(), Constants.space))
                    reader.SkipRegex(Constants.space);
                string element = reader.Read(regex);
                sequenceObj.Add(element);
            }
            reader.Dispose();
            return sequenceObj;
        }
    }
}
