/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Xtv.Windows
{
    using Parser;

    public partial class TraceBox : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Trace
        public static readonly DependencyProperty TraceProperty = DependencyProperty.Register("Trace", typeof(object), typeof(TraceBox), new UIPropertyMetadata(null, TraceChangedCallback));


        public Trace Trace
        {
            get { return (Trace)GetValue(TraceProperty); }
            set { SetValue(TraceProperty, value); }
        }

        private static void TraceChangedCallback(DependencyObject owner, DependencyPropertyChangedEventArgs args)
        {
            var window = (TraceBox)owner;
            var x = (Trace)args.NewValue;
        }
        #endregion

        public string Call
        {
            get
            {
                return Trace.Call;
            }
        }

        public bool HasParameters
        {
            get
            {
                return Trace.Parameters.Length > 0;
            }
        }

        public string Parameters
        {
            get
            {
                return Trace.Parameters.Length > 0 ? string.Join(", ", Trace.Parameters) : string.Empty;
            }
        }

        public string FileInfo
        {
            get
            {
                return Trace.FileName + " @ L" + Trace.FileLine;
            }
        }

        public bool IsExpandable
        {
            get
            {
                return Trace?.Children.Length > 0;
            }
        }
        private bool _childrenLoaded;
        private bool _isExpanded;
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (value && !_childrenLoaded)
                {
                    foreach (var child in Trace.Children)
                    {
                        Children.Children.Add(new TraceBox(child) { ProfileInfoVisible = _profileDataVisible });
                    }
                    _childrenLoaded = true;
                }
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
                OnPropertyChanged("Expanded");
            }
        }

        public Visibility Expanded
        {
            get
            {
                return IsExpandable && IsExpanded ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private bool _profileDataVisible = false;
        public Visibility ProfileInfoVisibility
        {
            get
            {
                return _profileDataVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool ProfileInfoVisible
        {
            get
            {
                return _profileDataVisible;
            }
            set
            {
                _profileDataVisible = value;
                if (_childrenLoaded)
                {
                    foreach (TraceBox child in Children.Children)
                    {
                        child.ProfileInfoVisible = _profileDataVisible;
                    }
                }
                OnPropertyChanged("ProfileInfoVisibility");
            }
        }

        public string TimingInfo
        {
            get
            {
                return string.Format("{0:0.000}ms / {1:0.000}ms", Trace.SelfTime * 1000, Trace.CumulativeTime * 1000);
            }
        }

        public double TotalTimingPercent
        {
            get
            {
                return Trace.TimePercent;
            }
        }

        public double ParentTimingPercent
        {
            get
            {
                return Trace.ParentTimePercent;
            }
        }

        public bool IsUserDefined
        {
            get
            {
                return Trace.IsUserDefined;
            }
        }

        public Brush BackColor
        {
            get
            {
                if (Trace.IsUserDefined)
                {
                    return Brushes.Aquamarine;
                }
                return Brushes.Beige;
            }
        }

        public TraceBox()
        {
            InitializeComponent();
            DataContext = this;
            IsExpanded = false;
        }

        public TraceBox(Trace trace) : this()
        {
            Trace = trace;
        }

        private void ExpandButton_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right || e.Key == Key.Left)
            {
                IsExpanded = e.Key == Key.Right;
                e.Handled = true;
            }
        }

        private void ParamsLabelAction(object sender, MouseButtonEventArgs e)
        {
            var dialog = new ParamsDialog(Trace.Parameters);
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }
    }
}
