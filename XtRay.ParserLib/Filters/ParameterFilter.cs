using System.Linq;
using System.Text.RegularExpressions;

namespace XtRay.Common.Filters
{
    using XtRay.Common.Abstractions;
    public class ParameterFilter : ITraceFilter
    {
        // parameters not support Full Match
        // support RegExp search
        private bool _regexp = false;
        private string _text;

        public ParameterFilter(string filterExpression)
        {
            if (!string.IsNullOrEmpty(filterExpression))
            {
                _text = filterExpression;
                if (filterExpression[0] == '/')
                {
                    _regexp = true;
                    _text = filterExpression.Substring(1);
                }
            }
        }
        public bool Apply(ITrace trace)
        {
            if (string.IsNullOrEmpty(_text))
            {
                return true;
            }

            if (trace.Parameters == null)
            {
                return false;
            }
            if (_regexp)
            {
                return Regex.IsMatch(string.Join("\n", trace.Parameters), _text);
            }

            return string.Join("\n", trace.Parameters).Contains(_text);
        }
    }
}