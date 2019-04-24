/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Gtk;

namespace XtRay.GtkSharp
{
    public static class GtkExtensions
    {
        public static void ApplyCss(this Widget widget, CssProvider provider, uint priority = uint.MaxValue)
        {
            widget.StyleContext.AddProvider(provider, priority);
            if (widget is Container container)
            {
                foreach (var child in container.Children)
                {
                    ApplyCss(child, provider, priority);
                }
            }
        }

    }
}
