/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Threading.Tasks;
using XtRay.Common.Abstractions;
using System.Reflection;
using XtRay.ParserLib;
using XtRay.ParserLib.Filters;

namespace XtRay.Windows
{
    public partial class MainWindow : Window
    {
        private readonly string WindowTitle;
        private TraceBox traceBox;
        private FlexibleTraceNode rootNode;
        private TraceTree parseResult;
        private bool ProfileInfoVisible = false;
        private bool RunParsingInParallel = true;
        private string lastOpenFile;
        #region Define parserList

        private ObservableCollection<string> ParserList = new ObservableCollection<string>
        {
            "Function", "Parameter", "Return"
        };

        #endregion

        public MainWindow()
        {
            WindowTitle = "XtRay v" + Assembly.GetExecutingAssembly().GetName().Version;
            Title = WindowTitle;
            InitializeComponent();
            var textChanged = ((EventHandler<EventArgs>)ApplyFilterEvent).Debounce(444);
            SearchBox.TextChanged += (s, e) => textChanged(s, e);
            ComboBox.SelectionChanged += (s, e) => textChanged(s, e);
            // support get args from startup
            string[] args = Environment.GetCommandLineArgs();
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
            StatusLabel.Content = "Loading file: " + filename;
            ParsingProgress.Visibility = Visibility.Visible;
            ParsingProgress.Value = 0;
            Task.Factory.StartNew(async () =>
            {
                var options = new ParserOptions
                {
                    ParseAsTree = true,
                    Parallel = RunParsingInParallel
                };
                var parser = Parser.FromFile(filename, options);
                await parser.PreParseAsync();
                Dispatcher.Invoke(() => ParsingProgress.Maximum = parser.LineCount);
                using (var t = new Timer((s) => { Dispatcher.Invoke(() => ParsingProgress.Value = parser.CurrentLine); }, null, 1, 100))
                {
                    parseResult = await parser.ParseAsync() as TraceTree;
                    Dispatcher.Invoke(() => ParsingProgress.Visibility = Visibility.Collapsed);
                }
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        traceBox = new TraceBox(parseResult.RootTrace) { ProfileInfoVisible = ProfileButton.IsChecked ?? false };
                        StatusLabel.Content = $"Done parsing in {parseResult.ParseDuration}";
                        rootNode = new FlexibleTraceNode(parseResult.RootTrace) { UiNode = traceBox };
                        TraceViewer.Content = traceBox;
                        Title = filename + " - " + WindowTitle;
                    }
                    catch (Exception ex)
                    {
                        StatusLabel.Content = "Failed to load file because: " + ex;
                    }
                });
            }, default, TaskCreationOptions.LongRunning, TaskScheduler.Current);
            lastOpenFile = filename;
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

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(lastOpenFile))
            {
                openFile(lastOpenFile);
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
            ProfileInfoVisible = !ProfileInfoVisible;
            if (TraceViewer.Content is TraceBox rootTrace)
            {
                rootTrace.ProfileInfoVisible = ProfileInfoVisible;
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

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var fd = new SaveFileDialog()
            {
                DefaultExt = "xt.json",
                Filter = "XtRay trace(JSON)|*.xt.json|XtRay trace(XML)|*.xt.xml|All Files (*.*)|*.*"
            };
            if (fd.ShowDialog(this) ?? false)
            {
                if (fd.FilterIndex == 2)
                {
                    Exporter.ExportXml(parseResult, fd.FileName);
                }
                else
                {
                    Exporter.ExportJson(parseResult, fd.FileName);
                }
            }

        }

        private void ParallelButton_Click(object sender, RoutedEventArgs e)
        {
            RunParsingInParallel = ParallelButton.IsChecked ?? false;
        }
    }
}
