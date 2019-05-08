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
        // TODO : I'm not sure how to Fix this stupid code TAT
        private readonly ObservableCollection<string> _parserList = new ObservableCollection<string>
        {
            "Function", "Parameter", "Return"
        };
        #endregion

        public MainWindow()
        {
            WindowTitle = "XtRay v" + Assembly.GetExecutingAssembly().GetName().Version;
            InitializeComponent();
            Title = WindowTitle;
            var textChanged = ((EventHandler<EventArgs>)ApplyFilterEvent).Debounce(444);
            SearchBox.TextChanged += (s, e) => textChanged(s, e);
            // support get args from startup
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && File.Exists(args[1]))
            {
                openFileWrapper(args[1]);
            }

            // TODO : I'm not sure which way is better to bind this to ItemSource
            ComboBox.ItemsSource = _parserList;
            ComboBox.SelectedIndex = 0;
            ComboBox.SelectionChanged += (s, e) => textChanged(s, e);
        }

        private void ApplyFilterEvent(object sender, EventArgs e)
        {
            int index = !ComboBox.Dispatcher.CheckAccess() ? ComboBox.Dispatcher.Invoke(() => ComboBox.SelectedIndex) : ComboBox.SelectedIndex;
            string text = !SearchBox.Dispatcher.CheckAccess() ? SearchBox.Dispatcher.Invoke(() => SearchBox.Text) : SearchBox.Text;

            // TODO : I'm not sure how to Fix this stupid code TAT
            switch (index)
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
            // Avoid the path or filename is tooooooo long
            StatusLabel.Content = "Loading file: " + filename.Ellipsize(100, true);
            Task.Factory.StartNew(async () =>
            {
                var options = new ParserOptions
                {
                    ParseAsTree = true,
                    Parallel = RunParsingInParallel
                };
                var parser = Parser.FromFile(filename, options);
                SetProgress(0);
                await parser.PreParseAsync();
                using (var t = new Timer((s) => SetProgress(parser.ParsingProgress), null, 1, 100))
                {
                    parseResult = await parser.ParseAsync() as TraceTree;
                    SetProgress(100);
                }
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        traceBox = new TraceBox(parseResult.RootTrace) { ProfileInfoVisible = ProfileButton.IsChecked ?? false };
                        StatusLabel.Content = $"Done parsing {parser.SourceLineCount} lines({parser.SourceLengthBytes} bytes) in {parseResult.ParseDuration}";
                        rootNode = new FlexibleTraceNode(parseResult.RootTrace) { UiNode = traceBox };
                        TraceViewer.Content = traceBox;
                        Title = WindowTitle + " - " + filename.Ellipsize(100, true);
                    }
                    catch (Exception ex)
                    {
                        StatusLabel.Content = "Failed to load file because: " + ex;
                    }
                });
            }, default, TaskCreationOptions.LongRunning, TaskScheduler.Current);
            lastOpenFile = filename;
            // Clear the searchBox text when open a new file
            SearchBox.Text = "";
        }

        private void SetProgress(double value)
        {
            Dispatcher.Invoke(() =>
            {
                if (value == 100)
                {
                    ParsingProgress.Visibility = Visibility.Collapsed;
                    TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                }
                else
                {
                    ParsingProgress.Visibility = Visibility.Visible;
                    ParsingProgress.Value = value;
                    TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                    TaskbarItemInfo.ProgressValue = value / 100;
                }
            });
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
            else
            {
                MessageBox.Show("Please Open a file so You Can Reload");
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
            // when no file opened , it can export a json file contains a 'null'
            if (parseResult == null)
            {
                MessageBox.Show("Please Open a file to Export");
                return;
            }
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
