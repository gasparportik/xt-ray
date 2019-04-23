using System.Linq;
using System.Text.RegularExpressions;

namespace XtRay.Common.Filters
{
    using XtRay.Common.Abstractions;
    public class ReturnValueFilter : ITraceFilter
    {
        // parameters not support Full Match
        // support RegExp search
        private bool _regexp = false;
        private string _text;

        public ReturnValueFilter(string filterExpression)
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
            if (trace.ReturnValue == null)
            {
                return false;
            }
            if (_regexp)
            {
                return Regex.IsMatch(trace.ReturnValue, _text);
            }

            return trace.ReturnValue.Contains(_text);
        }
    }
}