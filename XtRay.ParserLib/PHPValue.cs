
namespace XtRay.ParserLib
{
    public class PHPValue
    {
        public bool isParsed = false;
        private readonly string Value;
        private string ParsedValue = "";
        public PHPValue(string value)
        {
            Value = value;
        }

        // TODO : make it in Task...or something async
        // I dont know how to TAT
        public void Parse()
        {
            // tabs
            string tabs = "";
            // a ' is passed(means after this is a string)
            bool quoteStart = false;
            // a \ is passed(means after this need to be convert)
            bool flippedSlash = false;
            // a {}[];, is passed(means after this need a \n)
            bool isNewLine = false;
            foreach (char c in Value)
            {
                if (flippedSlash)
                {
                    ParsedValue += c;
                    flippedSlash = false;
                    continue;
                }

                if (quoteStart)
                {
                    ParsedValue += c;
                    if (c == '\\')
                    {
                        flippedSlash = true;
                    }
                    if (c == '\'')
                    {
                        quoteStart = false;
                    }
                    continue;
                }
                switch (c)
                {
                    case '(':
                    case '{':
                        tabs += "  ";
                        ParsedValue += c;
                        if (!isNewLine)
                        {
                            isNewLine = true;
                        }
                        break;
                    case ')':
                    case '}':
                        tabs = tabs.Substring(2);
                        ParsedValue += c;
                        if (!isNewLine)
                        {
                            isNewLine = true;
                        }
                        break;
                    case ',':
                    case ';':
                        ParsedValue += c;
                        if (!quoteStart)
                        {
                            isNewLine = true;
                        }
                        break;
                    case ' ':
                        ParsedValue += c;
                        break;
                    default:
                        quoteStart = c=='\'';
                        flippedSlash = c=='\\';
                        if (isNewLine)
                        {
                            isNewLine = false;
                            ParsedValue += "\n " + tabs;
                        }
                        ParsedValue += c;
                        continue;
                }
            }

            isParsed = true;
        }
        
        public override string ToString()
        {
            return isParsed ? ParsedValue : Value;
        }
    }
}