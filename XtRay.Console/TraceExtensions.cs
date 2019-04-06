/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using XtRay.ParserLib.Abstractions;

namespace XtRay.Console
{
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
                    return string.Format("{0,10} {1,10} {2}-> {3}() {4}:{5}", trace.TimeStart.ToString("0.0000"), trace.MemoryStart, Indent(trace.Level, ' '), trace.Call.Name, trace.File.Path, trace.FileLine);
                case DumpStyle.HumanReadableMinimal:
                    return string.Format("{0} {1}> {2} {3} @ L{4}", trace.Level.ToString("D3"), Indent(trace.Level), trace.Call.Name, trace.File.Path, trace.FileLine);
                case DumpStyle.Minimal:
                    return string.Format("{0} {1} {2} -> {3}", trace.Level.ToString("D3"), trace.Call?.Name ?? "MISSING_CALLNAME", trace.TimeStart, trace.TimeEnd);
                case DumpStyle.MinimalDebug:
                    return string.Format(">{0} #{1} {2} {3} @ L{4}", trace.Level, trace.CallIndex, trace.Call.Name, trace.File.Path, trace.FileLine);
                default:
                    return string.Empty;
            }
        }

        private static string Indent(int level, char filler = '-')
        {
            return new string(filler, level * 2);
        }

        internal enum DumpStyle
        {
            None = 0,
            Minimal = 1,
            MinimalDebug = 2,
            HumanReadableMinimal = 3,
            HumanReadable = 4
        }
    }
}
