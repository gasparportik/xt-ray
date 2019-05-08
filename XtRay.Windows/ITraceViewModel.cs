/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows;
using System.Windows.Media;

namespace XtRay.Windows
{
    public interface ITraceViewModel
    {
        string Call { get; }
        bool HasParameters { get; }
        string Parameters { get; }
        string ReturnValue { get; }
        string FileInfo { get; }
        Brush BackColor { get; }
        bool IsExpandable { get; }
        bool IsExpanded { get; set; }
        Visibility Expanded { get; }
        Visibility ProfileInfoVisibility { get; }
        string SelfTimeFormatted { get; } 
        string CumulativeTimeFormatted { get; } 
        double TotalTimingPercent { get; }
        double ParentTimingPercent { get; } 
    }
}
