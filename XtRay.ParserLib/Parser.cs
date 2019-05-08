/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace XtRay.ParserLib
{
    using Parsers;

    public abstract class Parser
    {
        protected ParserOptions Options;
        public uint SourceLineCount { get; protected set; }
        protected uint CurrentLine {get; set; }
        abstract public double ParsingProgress { get; }
        public uint SourceLengthBytes { get; protected set; }
        private readonly ConcurrentDictionary<string, TraceFile> fileMap = new ConcurrentDictionary<string, TraceFile>();
        private readonly ConcurrentDictionary<string, TraceCall> callMap = new ConcurrentDictionary<string, TraceCall>();

        protected Parser() { }

        internal TraceFile AddFile(string path) => fileMap.GetOrAdd(path, p => new TraceFile(p));

        internal TraceCall AddCall(string call) => callMap.GetOrAdd(call, c => new TraceCall(c));

        public TraceParseInfo PreParse()
        {
            return PreParseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public abstract Task<TraceParseInfo> PreParseAsync(CancellationToken ct = default);

        public TraceParseResult Parse()
        {
            return ParseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public abstract Task<TraceParseResult> ParseAsync(CancellationToken ct = default);

        public abstract string GetInfo();

        /// <summary>
        /// Tries to parse the specified call trace file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="ParserException">Thrown if the specified file has invalid format and therefore can't be parsed</exception>
        public static Parser FromFile(string file, ParserOptions options = null)
        {
            return new FileParser(file) { Options = options ?? new ParserOptions() };
        }

        public static Parser FromStream(Stream stream, ParserOptions options = null)
        {
            return new StreamParser(stream) { Options = options ?? new ParserOptions() };
        }

        public static Parser FromString(string content, ParserOptions options = null)
        {
            return new StreamParser(new MemoryStream(Encoding.UTF8.GetBytes(content), false)) { Options = options ?? new ParserOptions() };
        }
    }
}
