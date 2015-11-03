/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Xtv.Parser
{
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
                if (_formatVersion == 4)
                {
                    ParseData();
                }
                if (_formatVersion == 2)
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
            _data = new List<Trace>();
            var stack = new Stack<Trace>();
            Trace root = null;
            Trace last = null;
            using (var reader = File.OpenText(_file))
            {
                for (var i = 0; i < _startLine; ++i)
                {
                    reader.ReadLine();
                }
                foreach (var line in (from line in ReadLines(reader) select line))
                {
                    var parts = line.Split('\t');
                    try
                    {

                        if (parts[2] == "0")
                        {
                            var lastTrace = stack.Count > 0 ? stack.Peek() : null;
                            var trace = new Trace();
                            trace.parse(parts, root, lastTrace);
                            if (lastTrace != null)
                            {
                                lastTrace.addChild(trace);
                                stack.Push(trace);
                            }
                            else
                            {
                                _data.Add(trace);
                                stack.Push(trace);
                                root = trace;
                            }
                        }
                        else if (parts[2] == "1")
                        {
                            if (stack.Count > 0)
                            {
                                var lastTrace = stack.Pop();
                                last = lastTrace;
                                if (lastTrace.FunctionNumber == short.Parse(parts[1]))
                                {
                                    lastTrace.MemoryEnd = int.Parse(parts[4]);
                                    lastTrace.TimeEnd = float.Parse(parts[3]);
                                    lastTrace.close();
                                }
                                else
                                {
                                    lastTrace.close();
                                }
                            }
                        }
                        else if (parts[2] == "R")
                        {
                            if (last != null && last.ReturnValue == null)
                            {
                                last.ReturnValue = parts[5];
                            }
                        }
                        else if (parts[2] == "")
                        {
                            while (stack.Count > 0)
                            {
                                var trace = stack.Pop();
                                trace.MemoryEnd = int.Parse(parts[4]);
                                trace.TimeEnd = float.Parse(parts[3]);
                                trace.close();
                            }
                        }
                    }
                    catch (IndexOutOfRangeException ex)
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
                    stack.Pop().close();
                }
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
