/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XtRay.ParserLib.Parsers
{
    public class StreamParser : Parser
    {
        private readonly int _maxParallelism = Environment.ProcessorCount;
        private readonly Stream _stream;
        private readonly object _parallelListLock = new object();
        private byte _format;
        private string _version;
        private DateTime _date;
        private Stack<Trace> _stack;
        private Trace _last;
        private TraceParseResult _result;
        private DateTime _parseStart;
        private DateTime _parseEnd;
        private IList<Trace> _list;
        private Trace _rootTrace;

        public StreamParser(Stream stream)
        {
            _stream = stream;
        }

        public override async Task<TraceParseInfo> PreParseAsync(CancellationToken ct = default)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            ParseHeaders();
            LineCount = CountLines(_stream);
            await Task.Delay(1);
            return new TraceParseInfo
            {
                XdebugVersion = _version,
                TraceFormat = _format,
                TraceLines = LineCount,
                TraceDate = _date
            };
        }

        public override async Task<TraceParseResult> ParseAsync(CancellationToken ct = default)
        {
            try
            {
                _stream.Seek(0, SeekOrigin.Begin);
                ParseHeaders();
                // note that LineCount is not accurate(is higher than the actual count), but that doesn't really matter for its purpose
                LineCount = CountLines(_stream);
                _parseStart = DateTime.Now;
                _stream.Seek(0, SeekOrigin.Begin);
                await ParseData();
            }
            catch (Exception ex)
            {
                // TODO: collect errors and warnings
            }
            _parseEnd = DateTime.Now;
            _result.ParseDuration = _parseEnd - _parseStart;
            return _result;
        }

        private Trace CreateRootTrace()
        {
            return new Trace
            {
                Call = new TraceCall("root"),
                File = new TraceFile(this is FileParser fp ? fp.Filename : "???"),
                Level = 0,
            };
        }

        private async Task ParseData()
        {
            if (Options.Parallel)
            {
                if (Options.Experimental)
                {
                    ParseProducerConsumer();
                }
                else
                {
                    ParseParallel();
                }
            }
            else
            {
                await ParseLinear();
            }
        }

        private async Task ParseLinear()
        {
            _rootTrace = CreateRootTrace();
            _stack = new Stack<Trace>();
            _stack.Push(_rootTrace);
            using (var reader = new StreamReader(_stream, Encoding.UTF8, true, 4096))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    CurrentLine++;
                    ProcessLine(line);
                }
            }
            while (_stack.Count > 0)
            {
                _stack.Pop().DoneParsing();
            }
            _result = new TraceTree { RootTrace = _rootTrace };
        }

        private void ParseParallel()
        {
            _list = new List<Trace>(new Trace[LineCount]);
            _rootTrace = CreateRootTrace();
            var result = Parallel.ForEach(Lines(_stream).Skip(3), new ParallelOptions { MaxDegreeOfParallelism = _maxParallelism }, ProcessLineAsParallel);
            _list = _list.AsParallel().Where(x => x != null).OrderBy(x => x.SourceStartLine).ToList();
            if (Options.ParseAsTree)
            {
                BuildTreeFromList();
                _result = new TraceTree { RootTrace = _rootTrace };
            }
            else
            {
                _result = new TraceList(_list);
            }
        }

        private void ParseProducerConsumer()
        {
            const int boundedCapacity = 10000;
            _list = new List<Trace>(new Trace[LineCount]);

            var blockingCollection = new BlockingCollection<Tuple<int, string>>(boundedCapacity);
            Task.Run(() =>
            {
                int index = 0;
                using (var reader = new StreamReader(_stream, Encoding.UTF8, false, 4096, true))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        index++;
                        blockingCollection.Add(Tuple.Create(index, line));
                    }
                }
                blockingCollection.CompleteAdding();
            });

            blockingCollection
                .GetConsumingEnumerable()
                .AsParallel()
                .WithDegreeOfParallelism(_maxParallelism)
                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                .ForAll((item) => ProcessLineAsParallel(item.Item2, null, item.Item1));
            _list = _list.AsParallel().Where(x => x != null).ToList();
            if (Options.ParseAsTree)
            {
                BuildTreeFromList();
                _result = new TraceTree { RootTrace = _rootTrace };
            }
            else
            {
                _result = new TraceList(_list);
            }
        }

        private void ProcessLineAsParallel(string line, ParallelLoopState state, long lineIndex)
        {
            Trace trace;
            var parts = line.Split('\t');
            try
            {
                CurrentLine++;
                if (int.TryParse(parts[1], out var index)) 
                {
                    Console.WriteLine("OOOPPPSSSS!!!!!");
                    return;
                }
                lock (_parallelListLock)
                {

                    if (_list[index] == null)
                    {
                        _list[index] = trace = new Trace(_rootTrace);
                    }
                    else
                    {
                        trace = _list[index];
                    }
                }
                switch (parts[2])
                {
                    case "0":
                        trace.SourceStartLine = (uint)lineIndex;
                        ParseTraceEntry(trace, parts);
                        break;
                    case "1":
                        trace.SourceEndLine = (uint)lineIndex;
                        ParseTraceExit(trace, parts);
                        break;
                    case "R":
                        ParseTraceReturn(trace, parts);
                        break;
                    case "":
                        if (parts[0] == "" && parts[1] == "" && parts[3] != "")
                        {
                            ParseTraceExit(_rootTrace, parts);
                        }
                        break;
                }
            }
            catch (IndexOutOfRangeException)
            {
                if (parts[0].IndexOf("TRACE END", StringComparison.Ordinal) == 0)
                {
                    //break;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Parser exception: " + ex);
            }
        }

        private void BuildTreeFromList()
        {
            _stack = new Stack<Trace>();
            _stack.Push(_rootTrace);
            var level = -1;
            foreach (var trace in _list)
            {
                var lastTrace = _stack.Count > 0 ? _stack.Peek() : null;
                if (trace.Level > level)
                {
                    level = trace.Level;
                    if (lastTrace != null)
                    {
                        lastTrace.AppendChild(trace);
                    }
                    _stack.Push(trace);
                }
                else if (trace.Level == level)
                {
                    _stack.Pop().DoneParsing();
                    _stack.Peek().AppendChild(trace);
                    _stack.Push(trace);
                }
                else
                {
                    level = trace.Level;
                    if (_stack.Count > 0)
                    {
                        while (_stack.Peek().Level >= level)
                        {
                            _stack.Pop().DoneParsing();
                        }
                        _stack.Peek().AppendChild(trace);
                        _stack.Push(trace);
                    }
                }
            }
            while (_stack.Count > 0)
            {
                _stack.Pop().DoneParsing();
            }
        }

        private void ProcessLine(string line)
        {
            var parts = line.Split('\t');
            try
            {
                int index = -1;
                int.TryParse(parts[1], out index);
                switch (parts[2])
                {
                    case "0":
                        {
                            var lastTrace = _stack.Count > 0 ? _stack.Peek() : null;
                            var trace = new Trace(_rootTrace, lastTrace);
                            ParseTraceEntry(trace, parts);
                            if (lastTrace != null)
                            {
                                lastTrace.AppendChild(trace);
                            }
                            _stack.Push(trace);
                            break;
                        }
                    case "1":
                        if (_stack.Count > 0)
                        {
                            var lastTrace = _stack.Pop();
                            _last = lastTrace;
                            if (lastTrace.CallIndex == uint.Parse(parts[1], CultureInfo.InvariantCulture))
                            {
                                ParseTraceExit(_last, parts);
                                lastTrace.DoneParsing();
                            }
                            else
                            {
                                lastTrace.DoneParsing();
                            }
                        }
                        break;
                    case "R":
                        if (_last != null && _last.CallIndex == index)
                        {
                            ParseTraceReturn(_last, parts);
                        }
                        else
                        {
                            //something is in the wrong order
                        }
                        break;
                    case "":
                        while (_stack.Count > 0)
                        {
                            var trace = _stack.Pop();
                            ParseTraceExit(trace, parts);
                            trace.DoneParsing();
                        }
                        break;
                }
            }
            catch (IndexOutOfRangeException)
            {
                if (parts[0].IndexOf("TRACE END", StringComparison.Ordinal) == 0)
                {
                    //break;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Parser exception: " + ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ParseTraceEntry(Trace trace, string[] parts)
        {
            try
            {
                trace.Level = byte.Parse(parts[0], CultureInfo.InvariantCulture);
                trace.CallIndex = uint.Parse(parts[1], CultureInfo.InvariantCulture);
                trace.TimeStart = float.Parse(parts[3], CultureInfo.InvariantCulture);
                trace.MemoryStart = int.Parse(parts[4], CultureInfo.InvariantCulture);
                trace.Call = AddCall(parts[5]);
                trace.IsUserDefined = parts[6] == "1";
                if (parts[7].Length > 0)
                {
                    trace.ReferencedFile = AddFile(parts[7]);
                }
                trace.File = AddFile(parts[8]);
                trace.FileLine = short.Parse(parts[9], CultureInfo.InvariantCulture);
                if (parts.Length > 10)
                {
                    var paramCount = int.Parse(parts[10], CultureInfo.InvariantCulture);
                    if (paramCount > 0)
                    {
                        trace.Parameters = new string[paramCount];
                        for (var i = trace.Parameters.Length - 1; i >= 0; --i)
                        {
                            trace.Parameters[i] = parts[11 + i];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ParseTraceExit(Trace trace, string[] parts)
        {
            try
            {
                trace.MemoryEnd = int.Parse(parts[4], CultureInfo.InvariantCulture);
                trace.TimeEnd = float.Parse(parts[3], CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ParseTraceReturn(Trace trace, string[] parts)
        {
            try
            {
                trace.ReturnValue = parts[5];
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private void ParseHeaders()
        {
            string[] lines;
            try
            {
                using (var reader = new StreamReader(_stream, Encoding.UTF8, false, 4096, true))
                {
                    lines = new[] {
                        reader.ReadLine(),
                        reader.ReadLine(),
                        reader.ReadLine()
                    };
                }
                _stream.Seek(lines[0].Length + lines[1].Length + lines[2].Length + 4, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                throw new ParserException("File has too few lines. Expecting at least 6, 3 for header, 1 footer and at least one entry and one exit." + ex);
            }
            if (lines[0].IndexOf("Version: ", StringComparison.Ordinal) == 0
                && lines[1].IndexOf("File format: ", StringComparison.Ordinal) == 0 
                && lines[2].IndexOf("TRACE START", StringComparison.Ordinal) == 0)
            {
                _format = byte.Parse(lines[1].Replace("File format: ", ""));
                if (_format != 2 && _format != 4)
                {
                    throw new ParserException("Invalid 'File format' in header. Either a value of 4 or 2 is expected/supported.");
                }
                _version = lines[0].Replace("Version: ", "");
                if (!DateTime.TryParseExact(_version = lines[0].Replace("TRACE START ", ""), "[yyyy-MM-dd HH:mm:ss]", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out _date))
                {
                    _date = DateTime.Now;
                }
            }
            else
            {
                throw new ParserException("Invalid header lines. Expecting computer-readable format with 'Version', 'File format' and 'TRACE START' headers");
            }
        }

        private static uint CountLines(Stream stream)
        {
            uint lineCount = 0;
            var byteBuffer = new byte[4096 * 1024];
            int bytesRead;
            while ((bytesRead = stream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
            {
                var i = 0;
                for (; i < bytesRead; ++i)
                {
                    if (byteBuffer[i] == 10)
                    {
                        ++lineCount;
                    }
                }
            }
            return lineCount;
        }

        private static IEnumerable<string> Lines(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8, false, 4096, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public override string GetInfo()
        {
            return $"Parser running in mode {_format}/{_version}";
        }

    }
}
