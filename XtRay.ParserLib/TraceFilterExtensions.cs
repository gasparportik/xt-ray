/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtRay.ParserLib
{
    using Abstractions;

    public static class TraceFilterExtensions
    {

        public static bool ApplyFilter(this ITrace trace, ITraceFilter filter)
        {
            var matched = filter?.Apply(trace) ?? true;
            if (!matched)
            {
                foreach (var child in trace.Children)
                {
                    if (child.ApplyFilter(filter))
                    {
                        matched = true;
                        break;
                    }
                }
            }
            return matched;
        }
    }
}
