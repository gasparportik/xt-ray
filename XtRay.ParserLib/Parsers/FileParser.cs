/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace XtRay.Common.Parsers
{
    using Abstractions;

    public class FileParser : Parser
    {
        private string _file;
        private string _format;
        private byte _formatVersion;
        private byte _startLine;

        public FileParser(string file)
        {
            if (!File.Exists(file))
            {
                throw new IOException("The specified file does not exist: " + file + "!");
            }
            _file = file;
        }

        public void Parse()
        {
            Preparse();
            if (_format == "CR")
            {
                if (_formatVersion == 4 || _formatVersion == 2)
                {
                    ParseData();
                }
            }
            else
            {
                throw new ParserException("The file has an invalid format! Only computer-readable trace files are supported.");
            }
        }

        private void ParseData()
        {
            _list = new List<ITrace>();
            var stack = new Stack<Trace>();
            Trace last = null;
            using (var reader = File.OpenText(_file))
            {
                for (var i = 0; i < _startLine; ++i)
                {
                    reader.ReadLine();
                }
                foreach (var line in ReadLines(reader))
                {
                    var parts = line.Split('\t');
                    try
                    {
                        switch (parts[2])
                        {
                            case "0":
                                {
                                    var lastTrace = stack.Count > 0 ? stack.Peek() : null;
                                    var trace = new Trace(_rootTrace, lastTrace);
                                    _list.Add(trace);
                                    ParseTrace(trace, parts);
                                    if (lastTrace != null)
                                    {
                                        lastTrace.AppendChild(trace);
                                        stack.Push(trace);
                                    }
                                    else
                                    {
                                        stack.Push(trace);
                                        if (_rootTrace == null)
                                        {
                                            _rootTrace = trace;
                                        }
                                    }
                                    break;
                                }
                            case "1":
                                if (stack.Count > 0)
                                {
                                    var lastTrace = stack.Pop();
                                    last = lastTrace;
                                    if (lastTrace.CallIndex == ulong.Parse(parts[1], CultureInfo.InvariantCulture))
                                    {
                                        lastTrace.MemoryEnd = int.Parse(parts[4], CultureInfo.InvariantCulture);
                                        lastTrace.TimeEnd = float.Parse(parts[3], CultureInfo.InvariantCulture);
                                        lastTrace.DoneParsing();
                                    }
                                    else
                                    {
                                        lastTrace.DoneParsing();
                                    }
                                }
                                break;
                            case "R":
                                if (last != null && last.ReturnValue == null)
                                {
                                    last.ReturnValue = parts[5];
                                }
                                break;
                            case "":
                                while (stack.Count > 0)
                                {
                                    var trace = stack.Pop();
                                    trace.MemoryEnd = int.Parse(parts[4], CultureInfo.InvariantCulture);
                                    trace.TimeEnd = float.Parse(parts[3], CultureInfo.InvariantCulture);
                                    trace.DoneParsing();
                                }
                                break;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        if (parts[0].IndexOf("TRACE END") == 0)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        //Whoopsie daisies
                    }
                }
                while (stack.Count > 0)
                {
                    stack.Pop().DoneParsing();
                }
            }
        }

        private void ParseTrace(Trace trace, string[] parts)
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
                //let's ignore this for now
            }
        }

        private void Preparse()
        {
            var lines = (from line in ReadLines() select line).Take(3).ToList();
            if (lines[0].IndexOf("Version: ") == 0)
            {
                _format = "CR";
                if (lines[1].IndexOf("File format: ") == 0)
                {
                    _formatVersion = byte.Parse(lines[1].Replace("File format: ", ""));
                }
            }
            else
            {
                _format = "HR";
            }
            byte i = 0;
            foreach (var line in lines)
            {
                if (line.IndexOf("TRACE START") == 0)
                {
                    _startLine = (byte)(i + 1);
                    break;
                }
                ++i;
            }
        }

        private IEnumerable<string> ReadLines()
        {
            string line;
            using (var reader = File.OpenText(_file))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        private IEnumerable<string> ReadLines(StreamReader reader)
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        public override string GetInfo()
        {
            return $"Parser running in mode {_format}/{_formatVersion}";
        }
    }
}
