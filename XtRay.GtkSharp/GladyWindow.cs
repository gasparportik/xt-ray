/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Gtk;
using System.IO;

namespace XtRay.GtkSharp
{
    public class GladyWindow
    {
        public const string gladeElementId = "window1";
        public const bool useGlade = true;

        public GladyWindow(WindowType windowType, string filename)
        {
            if (useGlade)
            {
                using (var file = App.GetEmbeddedResourceStream("glade." + filename + ".glade"))
                {
                    Glade.XML gxml = new Glade.XML(file, gladeElementId, "");
                    gxml.Autoconnect(this);
                }
            }
            else
            {
                using (var file = App.GetEmbeddedResourceStream("glade." + filename + ".gtk.xml"))
                {
                    var builder = new Gtk.Builder();
                    using (var reader = new StreamReader(file))
                    {
                        builder.AddFromString(reader.ReadToEnd());
                        _window = builder.GetObject(gladeElementId) as Gtk.Window;
                    }
                }
            }
            Window.Icon = Gdk.Pixbuf.LoadFromResource(App.GetEmbeddedResourceName("resources.app.png"));
        }

        [Glade.Widget(gladeElementId)]
        private Window _window;
        public Window Window
        {
            get
            {
                return _window;
            }
            set
            {
                _window = value;
            }
        }

        public void Show()
        {
            _window.Show();
        }

        public void Destroy()
        {
            _window.Destroy();
        }

    }
}
