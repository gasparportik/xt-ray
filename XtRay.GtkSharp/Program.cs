/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using Gtk;
using System.IO;
using System.Reflection;

namespace XtRay.GtkSharp
{
    class App
    {
        private static Assembly exe;
        public static void Main(string[] args)
        {
            exe = Assembly.GetExecutingAssembly();
            Application.Init();
            MainWindow win = new MainWindow();
            win.Show();
            Application.Run();
        }

        internal static Stream GetEmbeddedResourceStream(string filename)
        {
            try
            {
                return exe.GetManifestResourceStream(GetEmbeddedResourceName(filename));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal static string GetEmbeddedResourceName(string filename)
        {
            return typeof(App).Namespace + "." + filename;
        }
    }
}
