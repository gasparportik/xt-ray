/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtRay.ParserLib.Abstractions;

namespace XtRay.ParserLib
{
    public class TraceList : TraceParseResult, IReadOnlyList<ITrace>
    {
        private IList<Trace> _list;
        internal TraceList(IList<Trace> list)
        {
            _list = list;
        }

        public ITrace this[int index] => _list[index];

        public int Count => _list.Count;

        public IEnumerator<ITrace> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }
}
