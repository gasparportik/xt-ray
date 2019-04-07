/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UI = Gtk.Builder.ObjectAttribute;
using Gtk;
using XtRay.ParserLib;
using System.Reflection;

namespace XtRay.GtkSharp
{

    public class MainWindow : Window
    {

        #region Glade widget instances
        [UI]
        private readonly ScrolledWindow ScrollViewer;
        [UI]
        private readonly ToggleButton ShowProfileButton;
        #endregion
        private readonly CssProvider css;

        private MainWindow(Builder builder) : base(builder.GetObject("MainWindow").Handle)
        {
            builder.Autoconnect(this);
        }

        public MainWindow() : this(new Builder("MainWindow.glade"))
        {
#if DEBUG
            if (File.Exists(@"c:\tmp\traces\trace.sample.xt"))
            {
                openFile(@"c:\tmp\traces\trace.sample.xt");
            }
#endif
            css = new CssProvider();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("app.css"))
            using (StreamReader reader = new StreamReader(stream))
            {
                css.LoadFromData(reader.ReadToEnd());
            }
            this.ApplyCss(css);
        }

        private void openFile(string filename)
        {
            Task.Factory.StartNew(async () =>
            {
                var parser = Parser.FromFile(filename, new ParserOptions { Parallel = true, ParseAsTree = true });
                var result = await parser.ParseAsync();
                ShowTraces(result as TraceTree);
            }, default, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        private void AboutButton_Clicked(object sender, EventArgs e)
        {
            var about = new AboutWindow
            {
                Parent = this,
                Icon = Icon,
            };
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
            var fileChooser = new FileChooserDialog("Pick a file", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
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
                    fileChooser.Destroy();
                    try
                    {
                        openFile(filename);
                    }
                    catch (Exception ex)
                    {
                        var dlg = new MessageDialog(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, "Failed to load/parse the selected file! \r\nSome indication: " + ex.Message);
                        dlg.Run();
                    }
                    break;
                case ResponseType.Cancel:
                case ResponseType.DeleteEvent:
                    fileChooser.Destroy();
                    break;
            }
        }

        protected void ReloadFileButton_Clicked(object sender, EventArgs e)
        {

        }

        protected void ExportFileButton_Clicked(object sender, EventArgs e)
        {

        }

        protected void MainWindow_DeleteEvent(object sender, DeleteEventArgs e)
        {
            Application.Quit();
            e.RetVal = true;
        }

        protected void ShowTraces(TraceTree result)
        {
            if (ScrollViewer.Child is Widget oldTraces)
            {
                ScrollViewer.Remove(oldTraces);
                oldTraces.Destroy();
            }
            try
            {
                var tb = new TraceBox(css, result.RootTrace) { ProfileInfoVisible = ShowProfileButton.State == StateType.Active };
                tb.Valign = Align.Start;
                ScrollViewer.AddWithViewport(tb);
                //var viewport = new Viewport();
                //viewport.Add(tb);
                //ScrollViewer.Add(viewport);
                tb.Show();
            }
            catch (Exception ex)
            {
                var bla = ex;
            }
        }
    }
}