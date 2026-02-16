using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlParser.src.xml
{
    internal enum ExternalId
    {
        SYSTEM,
        PUBLIC
    }

    internal struct XMLMetaData
    {
        // optional may only appear ones.
        // prologue
        string version;
        // optional
        Encoding encoding;
        // optional
        // Standalone Document Declaration
        bool standalone;

        // optional
        // processing instructions
        // starts with '<?' ends with '?>'
        // processing instructions target
        string PITarget;

        // optional
        // doctype declaration
        // must start with '<!DOCTYPE' and end with '>'
        string name;
        // optional
        // follows external id eather SYSTEM space ('"' [^"]* '"') | ("'" [^']* "'") or PUBLIC space '"' PubidChar* '"' | "'" (PubidChar - "'")* "'"
        // public char is #x20 | #xD | #xA | [a-zA-Z0-9] | [-'()+,./:=?;!*#@$_%]
        ExternalId externalId;
        // int subset
        // repeat of markup declaration or (pe declaration(% text ;) or space)
        // markupdecl = elementdecl | AttlistDecl | EntityDecl | NotationDecl | PI | Comment 
        // optional
        string[] internalSubSet;
    }
}
