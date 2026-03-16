using XmlParser.src.gui;

namespace XMLParser.src
{
    internal static class Constants
    {
        // made static since we only need one colorscheme for the entire program.
        public static readonly ColorScheme colorScheme = new ColorScheme();

        // For http requests
        public static readonly HttpClient httpClient = new HttpClient();

        // url regex
        public static readonly string url = "((([A-Za-z]{3,9}:(?:\\/\\/)?)(?:[-;:&=\\+\\$,\\w]+@)?[A-Za-z0-9.-]+|(?:www\\.|" +
            "[-;:&=\\+\\$,\\w]+@)[A-Za-z0-9.-]+)((?:\\/[\\+~%\\/.\\w-_]*)?\\??(?:[-\\+=&;%@.\\w_]*)#?(?:[\\w]*))?)";
        public static readonly string domain = "((([A-Za-z]{3,9}:(?:\\/\\/)?)(?:[-;:&=\\+\\$,\\w]+@)?[A-Za-z0-9.-]+|(?:www\\.|" +
            "[-;:&=\\+\\$,\\w]+@)[A-Za-z0-9.-]+)\\/)";

        // file regex
        public static readonly string filePath = "(^(?:[\\w]:(\\\\|\\/))(?:(?:\\.{1,2}(\\/|\\\\)))?(?:\\w+(\\/|\\\\))*)";
        public static readonly string file = $"(^(?:[\\w]:(\\\\|\\/))(?:(?:\\.{{1,2}}(\\/|\\\\)))?(?:\\w+(\\/|\\\\))*([^{Path.GetInvalidFileNameChars()}]))";

        // Indexing
        public static readonly string index = @"(\[[0-9]{,2}])";

        public static readonly char[] whiteSpace = [' ', '\t', '\r', '\n'];
    }
}
