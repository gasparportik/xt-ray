/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.Win32;
using System.Reflection;
using System.Windows;

namespace Xtv.Windows
{
    using Parser;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            var parser = Parser.ParseFile(@"C:\tmp\traces\trace.sample.xt");
            TraceViewer.Content = new TraceBox(parser.RootTrace);
#endif
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog()
            {
                CheckFileExists = true,
                DefaultExt = "xt",
                Multiselect = false,
                Filter = "Xdebug Trace files (*.xt)|*.xt|All Files (*.*)|*.*"
            };
            if (fd.ShowDialog(this) ?? false)
            {
                var parser = Parser.ParseFile(fd.FileName);
                TraceViewer.Content = new TraceBox(parser.RootTrace);
            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow();
            about.Owner = this;
            about.ShowDialog();
        }
    }
}
