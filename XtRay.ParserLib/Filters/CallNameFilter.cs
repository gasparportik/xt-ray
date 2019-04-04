/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
 
 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtRay.Common.Filters
{
    using XtRay.Common.Abstractions;
    public class CallNameFilter : ITraceFilter
    {
        private bool _exact = false;
        private string _text;

        public CallNameFilter(string filterExpression)
        {
            if (!string.IsNullOrEmpty(filterExpression) && filterExpression[0] == '=')
            {
                _exact = true;
                _text = filterExpression.Substring(1);
            }
            else
            {
                _text = filterExpression;
            }
        }

        public bool Apply(ITrace trace)
        {
            if (string.IsNullOrEmpty(_text))
            {
                return true;
            }
            return _exact ? trace.Call.Name == _text : trace.Call.Name.Contains(_text);
        }

    }
}
