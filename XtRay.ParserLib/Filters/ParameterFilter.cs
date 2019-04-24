/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Text.RegularExpressions;
using XtRay.ParserLib.Abstractions;

namespace XtRay.ParserLib.Filters
{
    public class ParameterFilter : ITraceFilter
    {
        // I think parameter doesnt need Full Match
        // support RegExp search
        private readonly bool _regexp = false;
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
            // CallName cant be none ,but others not
            if (trace.Parameters == null)
            {
                return false;
            }
            return _regexp ? Regex.IsMatch(string.Join("\n", trace.Parameters), _text) : string.Join("\n", trace.Parameters).Contains(_text);
        }
    }
}