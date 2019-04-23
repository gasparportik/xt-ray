/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using XtRay.Common.Abstractions;

namespace XtRay.Windows
{
    using Common.Parsers;
    using XtRay.Common;
    using XtRay.Common.Filters;

    public partial class MainWindow : Window
    {
        private TraceBox traceBox;
        private FlexibleTraceNode rootNode;
        #region Define parserList

        private ObservableCollection<string> ParserList = new ObservableCollection<string>
        {
            "Function", "Parameter", "Return"
        };

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            var textChanged = ((EventHandler<EventArgs>)ApplyFilterEvent).Debounce(444);
            SearchBox.TextChanged += (s, e) => textChanged(s, e);
            ComboBox.SelectionChanged += (s, e) => textChanged(s, e);
            // support get args from startup
            string[] args = Environment.GetCommandLineArgs();
            // index = 0 is the program self
            // index = 1 is the first param
            // ...etc
            if (args.Length > 1 && File.Exists(args[1]))
            {
                openFileWrapper(args[1]);
            }

            ComboBox.ItemsSource = ParserList;
        }

        private void ApplyFilterEvent(object sender, EventArgs e)
        {
            int _index = !ComboBox.Dispatcher.CheckAccess() ? ComboBox.Dispatcher.Invoke(() => ComboBox.SelectedIndex) : ComboBox.SelectedIndex;
            string text = !SearchBox.Dispatcher.CheckAccess() ? SearchBox.Dispatcher.Invoke(() => SearchBox.Text) : SearchBox.Text;
            switch (_index)
            {
                case 0:
                    rootNode.ApplyFilter(new CallNameFilter(text));
                    break;
                case 1:
                    rootNode.ApplyFilter(new ParameterFilter(text));
                    break;
                case 2:
                    rootNode.ApplyFilter(new ReturnValueFilter(text));
                    break;
            }
        }

        private void openFile(string filename)
        {
            var parser = Parser.ParseFile(filename);
            TraceViewer.Content = traceBox = new TraceBox(parser.RootTrace) { ProfileInfoVisible = ProfileButton.IsChecked ?? false };
            rootNode = new FlexibleTraceNode(parser.RootTrace) { UiNode = traceBox };
            SearchBox.Text = "";
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
                openFileWrapper(fd.FileName);
            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow();
            about.Owner = this;
            about.ShowDialog();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (TraceViewer.Content is TraceBox rootTrace)
            {
                rootTrace.ProfileInfoVisible = !rootTrace.ProfileInfoVisible;
            }
        }

        /**
         * the same open method
         */
        private void openFileWrapper(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    throw new FileNotFoundException();
                }
                openFile(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load/parse the selected file! \r\nSome indication: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /**
         * Support Drag file into the window and start parsing it
         */
        private void MainWindow_OnDragDrop(object sender, DragEventArgs e)
        {
            // Check is file drag in
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
                // if drag multiple files 
                if (file.Length > 1)
                {
                    MessageBox.Show("You can only drag one file into the window ^_^");
                    return;
                }
                openFileWrapper(file[0]);
            } 
        }
    }


}
