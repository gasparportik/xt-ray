/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using XtRay.ParserLib.Abstractions;
using System.Text.RegularExpressions;

namespace XtRay.ParserLib.Filters
{
    public class CallNameFilter : ITraceFilter
    {
        private readonly bool _exact = false;
        private readonly bool _regexp = false;
        private readonly string _text;

        public CallNameFilter(string filterExpression)
        {
            if (!string.IsNullOrEmpty(filterExpression))
            {
                if (filterExpression[0] == '=')
                {
                    _exact = true;
                    _text = filterExpression.Substring(1);
                }
                // Support RegExp Search
                else if (filterExpression[0] == '/')
                {
                    _regexp = true;
                    _text = filterExpression.Substring(1);
                }
                else
                {
                    _text = filterExpression;
                }
            }
        }

        public bool Apply(ITrace trace)
        {
            if (string.IsNullOrEmpty(_text))
            {
                return true;
            }

            if (_exact)
            {
                return trace.Call.Name == _text;
            }

            return _regexp ? Regex.IsMatch(trace.Call.Name, _text) : trace.Call.Name.Contains(_text);
        }
    }
}
