/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace XtRay.GtkSharp
{
    public class AboutWindow : AboutDialog
    {
        [UI]
        public TextView AboutText;

        private AboutWindow(Builder builder) : base(builder.GetObject("AboutWindow").Handle)
        {
            builder.Autoconnect(this);
        }

        public AboutWindow() : this(new Builder("AboutWindow.glade"))
        {
            Logo = Program.Logo.ScaleSimple(128, 128, Gdk.InterpType.Bilinear);
            Version = "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

    }
}