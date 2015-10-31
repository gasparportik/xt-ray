/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Xtv.Console
{
    using Parser;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                System.Console.WriteLine("Processing:" + args[0]);
                var parser = Parser.ParseFile(args[0]);
                System.Console.WriteLine(parser.GetInfo());
                System.Console.WriteLine();
                dump(new Trace[] { parser.RootTrace });
                
            }
            else
            {
                System.Console.WriteLine("No input file given.");
            }
        }

        static void dump(Trace[] traces)
        {
            foreach(var trace in traces)
            {
                System.Console.WriteLine(trace);
                if (trace.Children.Length > 0)
                {
                    dump(trace.Children);
                }
            }
        }
    }
}
