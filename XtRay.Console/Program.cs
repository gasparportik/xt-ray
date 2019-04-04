/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.IO;
using System.Diagnostics;
using Cout = System.Console;

namespace XtRay.Console
{
    using Common.Abstractions;
    using Common.Parsers;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Cout.WriteLine(string.Format("Usage: {0} <filename> [-n]", Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName)));
                Cout.WriteLine("The dump style arguments (n) are as follows: ");
                var index = 0;
                foreach (var style in Enum.GetNames(typeof(TraceExtensions.DumpStyle))) {
                    Cout.WriteLine($" -{index++} : {style}");
                }
                return;
            }
            try
            {
                var options = ParseArgs(args);
                Cout.WriteLine("Processing:" + options.Item1);
                var parser = Parser.ParseFile(options.Item1);
                Cout.WriteLine(parser.GetInfo());
                Cout.WriteLine();
                Dump(new ITrace[] { parser.RootTrace }, options.Item2);
            }
            catch (Exception ex)
            {
                Cout.WriteLine("Error: " + ex.Message);
            }
        }

        private static void Dump(ITrace[] traces, int style)
        {
            foreach (var trace in traces)
            {
                Cout.WriteLine(trace.ToString(style));
                if (trace.Children.Length > 0)
                {
                    Dump(trace.Children, style);
                }
            }
        }

        private static Tuple<string, int> ParseArgs(string[] args)
        {
            string file = null;
            int style = (int)TraceExtensions.DumpStyle.HumanReadable;
            foreach (var arg in args)
            {
                if (arg[0] == '-')
                {
                    style = int.Parse(arg.Substring(1));
                }
                else
                {
                    file = arg;
                }
            }
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new ArgumentException("You have to specify a trace file as input!");
            }
            return new Tuple<string, int>(file, style);
        }
    }
}
