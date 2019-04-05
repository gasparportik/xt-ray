/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace XtRay.Windows
{
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
            AppIcon.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/Resources/app.png", UriKind.RelativeOrAbsolute));
            DataContext = this;
        }

        private FlowDocument _aboutText;
        public FlowDocument AboutText
        {
            get
            {
                if (_aboutText == null)
                {
                    var sri = Application.GetResourceStream(new Uri("AboutText.xaml", UriKind.RelativeOrAbsolute));
                    using (var stream = sri.Stream)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            _aboutText = (FlowDocument)XamlReader.Parse(reader.ReadToEnd());
                            foreach (var link in FindHyperlinks(_aboutText))
                            {
                                link.RequestNavigate += HandleDocumentHyperLink;
                            }
                        }
                    }
                }
                return _aboutText;
            }
        }

        public string DisclaimerText { get; } = "Published under the Mozilla Public License v 2";

        public string TitleText
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string VersionText
        {
            get
            {
                return string.Format("Version {0}", Assembly.GetExecutingAssembly().GetName().Version);
            }
        }

        public string DescriptionText
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string ProductText
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string CopyrightText
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        private void HandleDocumentHyperLink(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        public IEnumerable<Hyperlink> FindHyperlinks(FlowDocument document)
        {
            return document.Blocks.SelectMany(FindElements<Hyperlink>);
        }

        private IEnumerable<T> FindElements<T>(Block block) where T : FrameworkContentElement
        {
            if (block is Table)
            {
                return ((Table)block).RowGroups
                    .SelectMany(x => x.Rows)
                    .SelectMany(x => x.Cells)
                    .SelectMany(x => x.Blocks)
                    .SelectMany(FindElements<T>);
            }
            if (block is Paragraph)
            {
                return ((Paragraph)block).Inlines.OfType<T>().Union(
                    ((Paragraph)block).Inlines.OfType<InlineUIContainer>().Where(x => x.Child is T).Select(x => x.Child as T)
                );
            }
            if (block is Section)
            {
                return ((Section)block).Blocks.SelectMany(FindElements<T>);
            }
            if (block is List)
            {
                return ((List)block).ListItems
                    .SelectMany(x => x.Blocks)
                    .SelectMany(FindElements<T>);
            }
            if (block is BlockUIContainer)
            {
                T i = ((BlockUIContainer)block).Child as T;
                return i == null ? new List<T>() : new List<T>(new[] { i });
            }
            throw new InvalidOperationException("Unknown block type: " + block.GetType());
        }
    }
}
