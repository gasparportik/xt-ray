using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtRay.Common
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
