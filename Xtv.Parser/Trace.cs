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
        public string ReturnValue { get; set; }
        public Trace[] Children { get; private set; }
        private Trace _root;
        private Trace _parent;
        private float _childrenTime;
        private IList<Trace> _children;

        public float SelfTime
        {
            get
            {
                return TimeEnd - Time - _childrenTime;
            }
        }

        public float CumulativeTime
        {
            get
            {
                return TimeEnd - Time;
            }
        }

        public double TimePercent
        {
            get
            {
                if (_root == null)
                {
                    return 0;
                }
                return 100 * CumulativeTime / _root.CumulativeTime;
            }
        }

        public double ParentTimePercent
        {
            get
            {
                if (_parent == null)
                {
                    return 0;
                }
                return 100 * CumulativeTime / _parent.CumulativeTime;
            }
        }

        internal void parse(string[] parts, Trace root = null, Trace parent = null)
        {
            try
            {

                _root = root;
                _parent = parent;
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
                if (parts.Length > 10)
                {
                    Parameters = new string[int.Parse(parts[10])];
                    for (var i = Parameters.Length - 1; i >= 0; --i)
                    {
                        Parameters[i] = parts[11 + i];
                    }
                }
                else
                {
                    Parameters = new string[0];
                }
            }
            catch (Exception ex)
            {
                //let's ignore this for now
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
                _childrenTime = _children.Sum(x => x.CumulativeTime);
            }
            else
            {
                Children = new Trace[0];
            }
        }

        public override string ToString()
        {
            return this.ToString(DumpStyle.MinimalDebug);
        }

        public string ToString(int style)
        {
            return this.ToString((DumpStyle)style);
        }

        public string ToString(DumpStyle style)
        {
            switch (style)
            {
                case DumpStyle.HumanReadable:
                    return string.Format("{0,10} {1,10} {2}-> {3}() {4}:{5}", Time.ToString("0.0000"), MemoryStart, indent(Level,' '), Call, FileName, FileLine);
                case DumpStyle.HumanReadableMinimal:
                    return string.Format("{0} {1}> {2} {3} @ L{4}", Level.ToString("D3"), indent(Level), Call, FileName, FileLine);
                case DumpStyle.Minimal:
                    return string.Format("{0} {1} {2} -> {3}", Level.ToString("D3"), Call, Time, TimeEnd);
                case DumpStyle.MinimalDebug:
                    return string.Format(">{0} #{1} {2} {3} @ L{4}", Level, FunctionNumber, Call, FileName, FileLine);
                default:
                    return "";
            }
        }

        static string indent(int level,char filler = '-')
        {
            return new string(filler, level * 2);
        }

        public enum DumpStyle
        {
            MinimalDebug = 0,
            Minimal = 1,
            HumanReadableMinimal = 2,
            HumanReadable = 3
        }
    }

}
