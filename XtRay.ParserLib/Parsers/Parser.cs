/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.Linq;

namespace XtRay.Common.Parsers
{
    using Abstractions;
    using System;

    public abstract class Parser
    {
        protected IList<ITrace> _list;
        internal Trace _rootTrace;
        public ITrace RootTrace
        {
            get
            {
                return _rootTrace;
            }
        }
        private Dictionary<string, TraceFile> fileMap = new Dictionary<string, TraceFile>();
        private Dictionary<string, TraceCall> callMap = new Dictionary<string, TraceCall>();

        protected Parser()
        {

        }

        public TraceFile AddFile(string path)
        {
            if (!fileMap.ContainsKey(path))
            {
                fileMap.Add(path, new TraceFile(path));
            }
            return fileMap[path];
        }

        public TraceCall AddCall(string call)
        {
            if (!callMap.ContainsKey(call))
            {
                callMap.Add(call, new TraceCall(call));
            }
            return callMap[call];
        }

        public abstract string GetInfo();

        /// <summary>
        /// Tries to parse the specified call trace file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="ParserException">Thrown if the specified file has invalid format and therefore can't be parsed</exception>
        public static Parser ParseFile(string file)
        {
            var parseStart = DateTime.Now;
            var parser = new FileParser(file);
            parser.Parse();
            var parseTime = DateTime.Now - parseStart;
            return parser;
        }
    }
}
