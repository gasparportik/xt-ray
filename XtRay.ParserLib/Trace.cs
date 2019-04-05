/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XtRay.Common
{
    using Abstractions;

    internal class Trace : ITrace
    {
        private static readonly ITrace[] EmptyTraceArray = new ITrace[0];

        public byte Level { get; internal set; }
        public uint CallIndex { get; internal set; }
        public TraceCall Call { get; internal set; }
        public string[] Parameters { get; internal set; }
        public bool IsUserDefined { get; internal set; }
        public string ReturnValue { get; internal set; }
        public TraceFile ReferencedFile { get; internal set; }

        public TraceFile File { get; internal set; }
        public short FileLine { get; internal set; }

        public int MemoryStart { get; internal set; }
        public int MemoryEnd { get; internal set; }

        public float TimeStart { get; internal set; }
        public float TimeEnd { get; internal set; }
        public ITrace[] Children { get; internal set; }

        public float SelfTime => TimeEnd - TimeStart - (Children?.Sum(x => x.CumulativeTime) ?? 0);

        public float CumulativeTime => TimeEnd - TimeStart;

        public float TimePercent => _root == null ? 0 : 100 * CumulativeTime / _root.CumulativeTime;

        public float ParentTimePercent => _parent == null ? 0 : 100 * CumulativeTime / _parent.CumulativeTime;

        private Trace _root;
        private Trace _parent;
        private IList<Trace> _children;

        //

        internal Trace(Trace root = null, Trace parent = null)
        {
            _root = root;
            _parent = parent;
        }

        internal void AppendChild(Trace child)
        {
            if (_children == null)
            {
                _children = new List<Trace>();
            }
            _children.Add(child);
        }

        internal void DoneParsing()
        {
            if (_children != null)
            {
                Children = _children.ToArray();
                _children = null;
            }
            else
            {
                Children = EmptyTraceArray;
            }
        }

    }

}
