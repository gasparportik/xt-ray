/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using Gtk;
using System.IO;
using System.Reflection;
using PBL = Gdk.PixbufLoader;

namespace XtRay.GtkSharp
{
    class Program
    {
        private static readonly Assembly exe = Assembly.GetExecutingAssembly();
        public static Gdk.Pixbuf Logo { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();

            var app = new Application("org.xtray.gui", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);
            
            var win = new MainWindow();
            app.AddWindow(win);
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("app.png"))
            using (var pbl = new PBL(stream))
            {
                Logo = pbl.Pixbuf;
            }
            win.Icon = Logo;
            win.Show();
            Application.Run();
        }
    }
}
