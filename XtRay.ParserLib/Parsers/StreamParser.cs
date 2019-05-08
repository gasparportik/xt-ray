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
        private static readonly Version VERSION_2_6 = new Version(2, 6);
        private readonly int _maxParallelism =
#if DEBUG
            1;
#else
            Environment.ProcessorCount;
#endif
        private readonly Stream _stream;
        private readonly object _parallelListLock = new object();
        private byte _format;
        private string _versionString;
        private Version _version;
        private DateTime _date;
        private Stack<Trace> _stack;
        private Trace _last;
        private TraceParseResult _result;
        private DateTime _parseStart;
        private DateTime _parseEnd;
        private IList<Trace> _list;
        private uint _processedListItems;
        private Trace _rootTrace;

        private byte _phase = 0;
        public override double ParsingProgress
        {
            get
            {
                return (_phase > 0 ? 5 : 1) + (CurrentLine / (double)SourceLineCount) * 85 + (_phase > 2 ? (_processedListItems / (double)_list.Count)*10 : 0);
            }
        }

        public StreamParser(Stream stream)
        {
            _stream = stream;
            SourceLengthBytes = (uint)stream.Length;
        }

        public override async Task<TraceParseInfo> PreParseAsync(CancellationToken ct = default)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            ParseHeaders();
            SourceLineCount = CountLines(_stream);
            await Task.Delay(1);
            return new TraceParseInfo
            {
                XdebugVersion = _versionString,
                TraceFormat = _format,
                TraceLines = SourceLineCount,
                TraceDate = _date
            };
        }

        public override async Task<TraceParseResult> ParseAsync(CancellationToken ct = default)
        {
            _parseStart = DateTime.Now;
            try
            {
                _phase = 0;
                _stream.Seek(0, SeekOrigin.Begin);
                ParseHeaders();
                // note that LineCount is not accurate(is higher than the actual count), but that doesn't really matter for its purpose
                SourceLineCount = CountLines(_stream);
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

        private void PostParseActions()
        {
            _rootTrace.TimeEnd = _rootTrace.Children.Sum(x => x.CumulativeTime);
            if (_version >= VERSION_2_6 || _rootTrace.Children.Length == 1)
            {
                _rootTrace = _rootTrace.Children[0] as Trace;
                //_rootTrace._parent = null;
            }
        }

        private async Task ParseLinear()
        {
            _phase = 1;
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
            PostParseActions();
            _result = new TraceTree { RootTrace = _rootTrace };
        }

        private void ParseParallel()
        {
            _phase = 1;
            _list = new List<Trace>(new Trace[SourceLineCount]);
            _rootTrace = CreateRootTrace();
            var result = Parallel.ForEach(Lines(_stream).Skip(3), new ParallelOptions { MaxDegreeOfParallelism = _maxParallelism }, ProcessLineAsParallel);
            _list = _list.AsParallel().Where(x => x != null).OrderBy(x => x.SourceStartLine).ToList();
            if (Options.ParseAsTree)
            {
                BuildTreeFromList();
                PostParseActions();
                _result = new TraceTree { RootTrace = _rootTrace };
            }
            else
            {
                _result = new TraceList(_list);
            }
        }

        private void ParseProducerConsumer()
        {
            _phase = 1;
            const int boundedCapacity = 10000;
            _list = new List<Trace>(new Trace[SourceLineCount]);

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
                PostParseActions();
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
                if (!int.TryParse(parts[1], out var index))
                {
                    // this is the last line of the trace, should be considered as exit line for all pending trace calls
                    if (parts.Length == 5 && parts[0] == "" && parts[1] == "" && parts[2] == "")
                    {
                        _list[0].SourceEndLine = (uint)lineIndex;
                        ParseTraceExit(_list[0], parts);
                    }
                    else
                    {
                        Console.WriteLine("OOOPPPSSSS!!!!!");
                    }
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
            _phase = 3;
            _stack = new Stack<Trace>();
            _stack.Push(_rootTrace);
            var level = -1;
            _processedListItems = 0;
            foreach (var trace in _list)
            {
                _processedListItems++;
                if (trace.SourceEndLine == 0)
                {
                    var main = _list[0];
                    trace.SourceEndLine = main.SourceEndLine;
                    trace.TimeEnd = main.TimeEnd;
                    trace.MemoryEnd = main.MemoryEnd;
                }
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
                            if (lastTrace.CallIndex == index)
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
                        // this is the last line of the trace, it should be considered as the exit for all pending trace calls
                        if (parts.Length == 5)
                        {
                            while (_stack.Count > 0)
                            {
                                var trace = _stack.Pop();
                                ParseTraceExit(trace, parts);
                                trace.DoneParsing();
                            }
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
                _versionString = lines[0].Replace("Version: ", "");
                if (!Version.TryParse(_versionString, out _version))
                {
                    _version = new Version(2, 0, 0);
                }
                if (!DateTime.TryParseExact(lines[2].Replace("TRACE START ", ""), "[yyyy-MM-dd HH:mm:ss]", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out _date))
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
            return $"Parser running in mode {_format}/{_versionString}";
        }

    }
}
