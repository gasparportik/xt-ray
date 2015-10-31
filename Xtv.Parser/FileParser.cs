/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                throw new ArgumentException("The specified file does not exist: " + file + "!");
            }
            this._file = file;
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
        }


        void ParseData()
        {
            _data = new List<Trace>();
            var stack = new Stack<Trace>();
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
                            trace.parse(parts);
                            if (lastTrace != null)
                            {
                                lastTrace.addChild(trace);
                                stack.Push(trace);
                            }
                            else
                            {
                                _data.Add(trace);
                                stack.Push(trace);
                            }
                        }
                        else if (parts[2] == "1")
                        {
                            var lastTrace = stack.Count > 0 ? stack.Pop() : null;
                            if (lastTrace != null)
                            {
                                lastTrace.MemoryEnd = int.Parse(parts[4]);
                                lastTrace.TimeEnd = float.Parse(parts[3]);
                                lastTrace.close();
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

        void Preparse()
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

        IEnumerable<string> ReadLines()
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

        IEnumerable<string> ReadLines(StreamReader reader)
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
