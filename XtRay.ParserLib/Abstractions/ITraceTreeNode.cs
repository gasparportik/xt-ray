/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace XtRay.Common.Abstractions
{
    public interface ITraceUiNode : INotifyPropertyChanged, IDisposable
    {
        ITrace Trace { get; }
        bool FilterMatched { set; }
        bool IsExpanded { get; set; }

        void ShowChildren(IEnumerable<FlexibleTraceNode> traces);
        
    }
}
