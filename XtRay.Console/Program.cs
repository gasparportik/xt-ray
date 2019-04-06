/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Mono.Options;
using System;
using System.Diagnostics;
using System.IO;
using XtRay.ParserLib.Abstractions;
using XtRay.ParserLib;
using Cout = System.Console;

namespace XtRay.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string file = null;
            bool wait = false;
            bool help = false;
            bool parallel = false;
            bool experimental = false;
            TraceExtensions.DumpStyle style = TraceExtensions.DumpStyle.None;
            var options = new OptionSet() {
                { "h|help",         "show this help message", v => help = true },
                { "p|parallel",     "use parallel parsing(will parse into list only)", v => parallel = true },
                { "w|wait",         "wait for keyboard input before terminating", v => wait = true },
                { "e|experimental", "enable some experimental stuff", v => experimental = true },
                //{ "1",              "dump trace as minimal list", v => style = TraceExtensions.DumpStyle.Minimal },
                //{ "2",              "dump trace as minimal human-readable list", v => style = TraceExtensions.DumpStyle.HumanReadableMinimal },
                //{ "3",              "dump trace as human readable list", v => style = TraceExtensions.DumpStyle.HumanReadable },
            };
            int index = 0;
            Func<int, Action<string>> styleSetter = idx => (v) => style = (TraceExtensions.DumpStyle)idx;
            foreach (var ds in Enum.GetNames(typeof(TraceExtensions.DumpStyle)))
            {
                options.Add(index.ToString(), ds, styleSetter(index));
                index++;
            }

            var extra = options.Parse(args);

            if (help)
            {
                WriteHelp(options);
                return;
            }

            if (extra.Count == 0)
            {
                WriteHelp(options);
                Environment.Exit(1);
            }
            else
            {
                file = extra[0];
            }

            if (!File.Exists(file))
            {
                Cout.WriteLine($"File {file} does not exist!");
                Environment.Exit(1);
            }

            Cout.WriteLine($"Processing: {file} using modes: basic{(parallel ? ",parallel" : "")}{(experimental ? ",experimental" : "")}");

            try
            {
                var parserOptions = new ParserOptions { Parallel = parallel, Experimental = experimental };
                var parser = Parser.FromFile(file, parserOptions);
                Cout.WriteLine(parser.GetInfo());
                var result = parser.Parse();
                Cout.WriteLine("Parsing is done. Took this long: " + result.ParseDuration);
                Cout.WriteLine();
                if (style != TraceExtensions.DumpStyle.None)
                {
                    Cout.WriteLine("Dumping parsed trace tree in the following style:" + style.ToString());
                    if (wait)
                    {
                        Cout.WriteLine("Last chance to Ctrl-C!");
                        Cout.ReadKey();
                    }
                    if (result is TraceList list)
                    {
                        DumpList(list, style);
                    }
                    else if (result is TraceTree tree)
                    {
                        DumpTree(new ITrace[] { tree.RootTrace }, style);
                    }
                }
            }
            catch (Exception ex)
            {
                Cout.WriteLine("Error: " + ex.Message);
            }
            if (wait)
            {
                Cout.ReadKey();
            }
        }

        private static void WriteHelp(OptionSet options)
        {
            Cout.WriteLine(string.Format("Usage: {0} [options] <filename>", Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName)));
            Cout.WriteLine("Available options are: ");
            options.WriteOptionDescriptions(Cout.Out);
        }

        private static void DumpList(TraceList list, TraceExtensions.DumpStyle style)
        {
            foreach (var trace in list)
            {
                try
                {
                    Cout.WriteLine(trace.ToString(style));
                }
                catch (Exception ex)
                {
                    Cout.Error.WriteLine(ex);
                }
            }
        }

        private static void DumpTree(ITrace[] traces, TraceExtensions.DumpStyle style)
        {
            foreach (var trace in traces)
            {
                Cout.WriteLine(trace.ToString(style));
                if (trace.Children.Length > 0)
                {
                    DumpTree(trace.Children, style);
                }
            }
        }
    }
}
