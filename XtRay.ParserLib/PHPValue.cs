
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
            // avoid duplicated parse
            if (isParsed)
            {
                return;
            }
            // tabs
            string tabs = "";
            // a ' is passed(means after this is a string)
            // in xdebug string must be quoted with ''
            bool quoteStart = false;
            // a \ is passed(means after this need to be convert)
            bool flippedSlash = false;
            // a {}[];, is passed(means after this need a \n)
            bool isNewLine = false;
            foreach (char c in Value)
            {
                // if the char need to convert
                if (flippedSlash)
                {
                    // just add and do next
                    ParsedValue += c;
                    flippedSlash = false;
                    continue;
                }
                // if the char in quote
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
                        // don't change anything with a space
                        ParsedValue += c;
                        break;
                    default:
                        quoteStart = c=='\'';
                        flippedSlash = c=='\\';
                        // if this char is not special just give a new line to it
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

        public void UnParse()
        {
            isParsed = false;
        }
        
        public override string ToString()
        {
            return isParsed ? ParsedValue : Value;
        }
    }
}