/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace XtRay.GtkSharp
{
    public class AboutWindow : GladyWindow
    {
        [Glade.Widget]
        public Gtk.TextView AboutText;

        public AboutWindow() : base(Gtk.WindowType.Toplevel, "AboutWindow")
        {
            ((Gtk.AboutDialog)Window).Version = "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        internal int Run()
        {
            return ((Gtk.AboutDialog)Window).Run();
        }
    }
}