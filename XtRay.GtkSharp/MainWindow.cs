/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using Gtk;

namespace XtRay.GtkSharp
{
    using Common.Parsers;
    using System.IO;
    public class MainWindow : GladyWindow
    {

        #region Glade widget instances
#pragma warning disable 0649

        [Glade.Widget("ScrollViewer")]
        private ScrolledWindow scroller;

        [Glade.Widget("ShowProfileButton")]
        private ToggleButton profileButton;

#pragma warning restore 0649
        #endregion

        public MainWindow() : base(WindowType.Toplevel, "MainWindow")
        {
#if DEBUG
            if (File.Exists(@"c:\tmp\traces\trace.sample.xt")) {
                ShowTraces(Parser.ParseFile(@"c:\tmp\traces\trace.sample.xt"));
            }
#endif
        }

        private void AboutButton_ClickEvent(object sender, EventArgs e)
        {
            var about = new AboutWindow();
            var result = (ResponseType)Enum.ToObject(typeof(ResponseType), about.Run());
            switch (result)
            {
                case ResponseType.DeleteEvent:
                case ResponseType.Cancel:
                    about.Destroy();
                    break;
            }
        }

        private void OpenFileButton_Clicked(object sender, EventArgs e)
        {
            var fileChooser = new FileChooserDialog("Pick a file", this.Window, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
            fileChooser.Filter = new FileFilter();
            fileChooser.Filter.AddPattern("*.xt");
#if DEBUG
            fileChooser.SetCurrentFolder(@"c:\tmp\traces");
#endif
            var result = (ResponseType)Enum.ToObject(typeof(ResponseType), fileChooser.Run());
            switch (result)
            {
                case ResponseType.Accept:
                    var filename = fileChooser.Filename;
                    Parser parser = null;
                    fileChooser.Destroy();
                    try
                    {
                        parser = Parser.ParseFile(filename);
                    }
                    catch (Exception ex)
                    {
                        var dlg = new MessageDialog(this.Window, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, "Failed to load/parse the selected file! \r\nSome indication: " + ex.Message);
                        dlg.Run();
                    }
                    if (parser != null)
                    {
                        ShowTraces(parser);
                    }
                    break;
                case ResponseType.Cancel:
                case ResponseType.DeleteEvent:
                    fileChooser.Destroy();
                    break;
            }
        }

        protected void MainWindow_DeleteEvent(object sender, DeleteEventArgs e)
        {
            Application.Quit();
            e.RetVal = true;
        }

        protected void ShowTraces(Parser parser)
        {
            var oldTraces = scroller.Child as Widget;
            if (oldTraces != null)
            {
                scroller.Remove(oldTraces);
                oldTraces.Destroy();
            }
            var tb = new TraceBox(parser.RootTrace) { Trace = parser.RootTrace, ProfileInfoVisible = profileButton.State == StateType.Active };
            scroller.AddWithViewport(tb);
            tb.Show();
        }
    }
}