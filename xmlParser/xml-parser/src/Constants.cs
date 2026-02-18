using System;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace XmlParser.src
{
    internal static class Constants
    {
        // made static since we only need one colorscheme for the entire program.
        public static readonly ColorScheme colorScheme = new ColorScheme();

        // For http requests
        public static readonly HttpClient httpClient = new HttpClient();

        // url regex
        public static readonly string url = "((([A-Za-z]{3,9}:(?:\\/\\/)?)(?:[-;:&=\\+\\$,\\w]+@)?[A-Za-z0-9.-]+|(?:www.|" +
            "[-;:&=\\+\\$,\\w]+@)[A-Za-z0-9.-]+)((?:\\/[\\+~%\\/.\\w-_]*)?\\??(?:[-\\+=&;%@.\\w_]*)#?(?:[\\w]*))?)";
        public static readonly string domain = "((([A-Za-z]{3,9}:(?:\\/\\/)?)(?:[-;:&=\\+\\$,\\w]+@)?[A-Za-z0-9.-]+|(?:www.|" +
            "[-;:&=\\+\\$,\\w]+@)[A-Za-z0-9.-]+)\\/)";

        // file regex
        public static readonly string filePath = "(^(?:(?:\\.{1,2}\\/))?(?:\\w+\\/)*)";

        // Character Range
        public static readonly string chararacter = "(\\t|\\n|\\r|[\\u0020-\\uD7FF]|[\\uE000-\\uFFFD])";

        // White Space
        public static readonly string space = "((\\s|\\t|\\r|\\n)+)";

        // Names and Tokens
        public static readonly string nameStartChar = "(:|[A-Z]|_|[a-z]|[\\u00C0-\\u00D6]|[\\u00D8-\\u00F6]|[\\u00F8-\\u02FF]|[\\u0370-\\u037D]|" +
            "[\\u037F-\\u1FFF]|[\\u200C-\\u200D]|[\\u2070-\\u218F]|[\\u2C00-\\u2FEF]|[\\u3001-\\uD7FF]|[\\uF900-\\uFDCF]|[\\uFDF0-\\uFFFD])";
        public static readonly string nameChar = $"({nameStartChar}|-|\\.|[0-9]|·|[\\u0300-\\u036F]|[\\u203F-\\u2040])";
        public static readonly string name = $"({nameStartChar}({nameChar})*)";
        //public static readonly string name = $"(.*)";
        public static readonly string names = $"({name}(\\s{name})*)";
        public static readonly string nmToken = $"(({nameChar})+)";
        public static readonly string nmTokens = $"({nmToken}(\\s{nmToken})*)";

        // Literals
        public static readonly string entityValue = $"(\\\"([^%&\\\"]|({peReference})|({reference}))*\\\")|(\\'([^%&\\']|({peReference})|({reference}))*\\')";
        public static readonly string attValue = $"(\\\"([^<&\\\"]|({reference}))*\\\")|(\\'([^<&\\']|({reference}))*\\')";
        public static readonly string systemLiteral = $"(\\\"[^\\\"]*\\\")|(\\'[^\\']*\\')";
        public static readonly string pubidChar = "(\\s|\\r|\\n|[a-zA-Z0-9]|[-'()+,./:=?;!*#@$_%])";
        public static readonly string pubidLiteral = $"((\\\"({pubidChar})*\\\")|(\\'({pubidChar.Replace("'", "")})*\\'))";

        // Character Data
        public static readonly string charData = "([^<&]*(?![^<&]*]]>[^<&]*))";

        // Comments
        public static readonly string comment = $"(<!--(({chararacter.Replace("-", "")})|(-({chararacter.Replace("-", "")})))*-->)";

        // Processing Instructions
        public static readonly string pi = $"<\\?{piTarget}({space}({chararacter.Replace(")*", "(?!\\?>))*")}*{chararacter.Replace("*", "")}))?\\?>";
        public static readonly string piTarget = $"((?!x|X)(?!m|M)(?!l|L)){name.Replace(")*", "(?!(x|X)(m|M)(l|L)))*")}";

        // CDATA Sections
        public static readonly string cdSect = $"{cdStart}{cData}{cdEnd}";
        public static readonly string cdStart = "<!\\[CDATA\\[";
        public static readonly string cData = $"(?:(?:{chararacter.Replace(")*", "(?!]]>))*")}{chararacter.Replace("*", "")})";
        public static readonly string cdEnd = "]]>";

        // Prolog
        public static readonly string prolog = $"{xmlDec}?{misc}*({doctypedecl}{misc}*)?";
        public static readonly string xmlDec = $"<\\?xml{versionInfo}{encodingDecl}?{sdDecl}?{space}?\\?>";
        public static readonly string versionInfo = $"{space}version{equals}((\\'{versionNum}\\')|(\\\"{versionNum}\\\"))";
        public static readonly string equals = $"{space}?={space}?";
        public static readonly string versionNum = "1\\.[0-9]+";
        public static readonly string misc = $"{comment}|{pi}|{space}";

        // Document Type Definition
        public static readonly string doctypedecl = $"<!DOCTYPE{space}{name}({space}{externalId})?{space}?(\\[{intSubset}]{space}?)?>";
        public static readonly string declSep = $"{peReference}|{space}";
        public static readonly string intSubset = $"(({markupdecl})|({declSep}))*";
        public static readonly string markupdecl = $"({elementdecl})|({attlistDecl})|({entityDecl})|({notationDecl})|({pi})|({comment})";

        // External Subset
        public static readonly string extSubset = $"{textDecl}?{extSubsetDecl}";
        public static readonly string extSubsetDecl = $"(({markupdecl})|({conditionalSect})|({declSep}))*";

        // Standalone Document Declaration
        public static readonly string sdDecl = $"{space}standalone{equals}((\\'(yes|no)\\')|(\\\"(yes|no)\\\"))";

        // Element
        public static readonly string element = $"({emptyElemTag})|({sTag}{content}{eTag})";

        // Start-tag
        public static readonly string sTag = $"<{name}({space}{attribute})*{space}?>";
        public static readonly string attribute = $"{name}{equals}{attValue}";

        // End-tag
        public static readonly string eTag = $"<\\/{name}{space}?>";

        // Content of Elements
        public static readonly string content = $"{charData}?((({element})|({reference})|({cdSect})|({pi})|({comment}))({charData})?)*";

        // Tags for Empty Elements
        public static readonly string emptyElemTag = $"<{name}({space}{attribute})*{space}?\\/>";

        // Element Type Declaration
        public static readonly string elementdecl = $"<!ELEMENT{space}{name}{space}{contentSpec}{space}?>";
        public static readonly string contentSpec = $"(EMPTY)|(ANY)|({mixed})|({children})";


        // Element-content Models
        public static readonly string children = $"(({choice})|({seq}))(\\?|\\*||\\+)?";
        public static readonly string cp = $"(({name})|({choice})|({seq}))(\\?|\\*||\\+)?";
        public static readonly string choice = $"\\({space}?{cp}({space}?\\|{space}?{cp})+{space}?\\)";
        public static readonly string seq = $"\\({space}?{cp}({space}?,{space}?{cp})+{space}?\\)";

        // Mixed-content Declaration
        public static readonly string mixed = $"(\\({space}?#PCDATA({space}?\\|{space}?{name})*{space}?\\)\\*)|\\({space}#PCDATA{space}?\\)";

        // Attribute-list Declaration
        public static readonly string attlistDecl = $"<!ATTLIST{space}{name}({attDef})*{space}?>";
        public static readonly string attDef = $"{space}{name}{space}{attType}{space}{defaultDecl}";

        // Attribute Types
        public static readonly string attType = $"({stringType})|({tokenizedType})|({enumerationType})";
        public static readonly string stringType = "CDATA";
        public static readonly string tokenizedType = "(ID)|(IDREF)|(IDREFS)|(ENTITY)|(ENTITIES)|(NMTOKEN)|(NMTOKENS)";

        // Enumerated Attribute Types
        public static readonly string enumerationType = $"({notationType})|({enumeration})";
        public static readonly string notationType = $"NOTATION{space}\\({space}?{name}({space}?\\|{space}?{name})*{space}?\\)";
        public static readonly string enumeration = $"\\({space}?{nmToken}({space}?\\|{space}?{nmToken})*{space}?\\)";

        // Attribute Defaults
        public static readonly string defaultDecl = $"(#REQUIRED)|(#IMPLIED)|((#FIXED{space})?{attValue})";

        // Conditional Section
        public static readonly string conditionalSect = $"({includeSect})|({ignoreSect})";
        public static readonly string includeSect = $"<!\\[{space}?INCLUDE{space}?\\[{extSubsetDecl}]]>";
        public static readonly string ignoreSect = $"'<!\\[{space}?IGNORE{space}?\\[{ignoreSectContents}*]]>";
        public static readonly string ignoreSectContents = $"{ignore}({ignore})*";
        public static readonly string ignore = $"{chararacter.Replace(")*", "(?!(<!\\[)|(]]>)))*")}{chararacter.Replace("*", "")}";

        // Character Reference
        public static readonly string charRef = $"(&#[0-9]+;)|(&#x[0-9a-fA-F]+;)";

        // Entity Reference
        public static readonly string reference = $"({entityRef})|({charRef})";
        public static readonly string entityRef = $"&{name};";
        public static readonly string peReference = $"%{name};";

        // External Entity Declaration
        public static readonly string externalId = $"(SYSTEM{space}{systemLiteral})|(PUBLIC{space}{pubidLiteral}{space}{systemLiteral})";
        public static readonly string nDataDecl = $"{space}NDATA{space}{name}";

        // Entity Declaration
        public static readonly string entityDef = $"({entityValue})|({externalId}{nDataDecl}?)";
        public static readonly string peDef = $"(({entityValue})|({externalId}))";
        public static readonly string geDecl = $"<!ENTITY{space}{name}{space}{entityDef}{space}?>";
        public static readonly string peDecl = $"<!ENTITY{space}%{space}{name}{space}{peDef}{space}?>";
        public static readonly string entityDecl = $"({geDecl})|({peDecl})";

        // Text Declaration
        public static readonly string textDecl = $"<\\?xml{versionInfo}?{encodingDecl}{space}?\\?>";

        // Well-Formed External Parsed Entity
        public static readonly string extParsedEnt = $"{textDecl}?{content}";

        // Encoding Declaration
        public static readonly string encodingDecl = $"{space}encoding{equals}((\\\"{encName}\\\")|('{encName}'))";
        public static readonly string encName = $"[A-Za-z]([A-Za-z0-9._]|-)*";

        // Notation Declarations
        public static readonly string notationDecl = $"<!NOTATION{space}{name}{space}({externalId}|{publicId}){space}?>";
        public static readonly string publicId = $"PUBLIC{space}{pubidLiteral}";

        // Document
        public static readonly string document = $"(({prolog})({element})({misc})*)";

        public static string RegexReplace(string value, string replacement, string regex)
        {
            var replace = new Regex(regex);
            return replace.Replace(value, replacement);
        }

        public static bool RegexMatch(string value, string regex)
        {
            var reg = new Regex(regex);
            return reg.IsMatch(value);
        }

        public static int RegexCount(string value, string regex)
        {
            var reg = new Regex(regex);
            MatchCollection matches = reg.Matches(regex);
            return matches.Count;
        }

        public static string RegexExtract(string value, string regex)
        {
            var reg = new Regex(regex);
            Match match = reg.Match(value);
            return match.Value;
        }
    }
}