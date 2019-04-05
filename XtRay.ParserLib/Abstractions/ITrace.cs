/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtRay.Common.Abstractions
{
    public interface ITrace
    {
        byte Level { get; }
        uint CallIndex { get; }
        TraceCall Call { get; }
        bool IsUserDefined { get; }
        string[] Parameters { get; }
        string ReturnValue { get; }
        TraceFile ReferencedFile { get; }

        TraceFile File { get; }
        short FileLine { get; }

        int MemoryEnd { get; }
        int MemoryStart { get; }

        float TimeStart { get; }
        float TimeEnd { get; }
        float SelfTime { get; }
        float CumulativeTime { get; }
        float TimePercent { get; }
        float ParentTimePercent { get; }

        [NotNull]
        ITrace[] Children { get; }
    }
}
