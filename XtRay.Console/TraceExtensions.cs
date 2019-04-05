using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtRay.Console
{
    using Common.Abstractions;

    internal static class TraceExtensions
    {

        public static string ToString(this ITrace trace)
        {
            return ToString(trace, DumpStyle.MinimalDebug);
        }

        public static string ToString(this ITrace trace, int style)
        {
            return ToString(trace, (DumpStyle)style);
        }

        public static string ToString(this ITrace trace, DumpStyle style)
        {
            switch (style)
            {
                case DumpStyle.HumanReadable:
                    return string.Format("{0,10} {1,10} {2}-> {3}() {4}:{5}", trace.TimeStart.ToString("0.0000"), trace.MemoryStart, indent(trace.Level, ' '), trace.Call, trace.File.Path, trace.FileLine);
                case DumpStyle.HumanReadableMinimal:
                    return string.Format("{0} {1}> {2} {3} @ L{4}", trace.Level.ToString("D3"), indent(trace.Level), trace.Call, trace.File.Path, trace.FileLine);
                case DumpStyle.Minimal:
                    return string.Format("{0} {1} {2} -> {3}", trace.Level.ToString("D3"), trace.Call, trace.TimeStart, trace.TimeEnd);
                case DumpStyle.MinimalDebug:
                    return string.Format(">{0} #{1} {2} {3} @ L{4}", trace.Level, trace.CallIndex, trace.Call, trace.File.Path, trace.FileLine);
                default:
                    return "";
            }
        }

        private static string indent(int level, char filler = '-')
        {
            return new string(filler, level * 2);
        }

        internal enum DumpStyle
        {
            MinimalDebug = 0,
            Minimal = 1,
            HumanReadableMinimal = 2,
            HumanReadable = 3
        }
    }
}
