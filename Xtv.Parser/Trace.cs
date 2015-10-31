/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xtv.Parser
{
    public class Trace
    {
        public byte Level { get; set; }
        public short FunctionNumber { get; set; }
        public float Time { get; set; }
        public float TimeEnd { get; set; }
        public int MemoryStart { get; set; }
        public int MemoryEnd { get; set; }
        public string Call { get; set; }
        public bool IsUserDefined { get; set; }
        public string FileName { get; set; }
        public short FileLine { get; set; }
        public string[] Parameters { get; set; }
        public Trace[] Children { get; private set; }
        private IList<Trace> _children;

        internal void parse(string[] parts)
        {
            Level = byte.Parse(parts[0]);
            FunctionNumber = short.Parse(parts[1]);
            Time = float.Parse(parts[3]);
            MemoryStart = int.Parse(parts[4]);
            Call = parts[5];
            IsUserDefined = parts[6] == "1";
            if (parts[7].Length > 0)
            {
                Call += "(" + parts[7] + ")";
            }
            FileName = parts[8];
            FileLine = short.Parse(parts[9]);
            Parameters = new string[int.Parse(parts[10])];
            for (var i = Parameters.Length -1 ; i >= 0; --i)
            {
                Parameters[i] = parts[11 + i];
            }
        }

        internal void addChild(Trace child)
        {
            if (_children == null)
            {
                _children = new List<Trace>();
            }
            _children.Add(child);
        }
        internal void close()
        {
            if (_children != null)
            {
                Children = _children.ToArray();
            } else
            {
                Children = new Trace[0];
            }
        }

        public override string ToString()
        {
            return $"{Level} {indent(Level)} {Call} {FileName} {FileLine}";
        }

        static string indent(int level)
        {
            return new String(' ', level*2);
        }
    }
}
